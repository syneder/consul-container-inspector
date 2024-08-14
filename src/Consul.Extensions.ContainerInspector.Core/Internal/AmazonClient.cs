using Amazon.Runtime;
using Amazon.Runtime.Internal.Auth;
using Amazon.Util;
using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core.Internal.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using Consul.Extensions.ContainerInspector.Extensions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IAmazonClient" />.
    /// </summary>
    internal class AmazonClient(
        IHttpClientFactory clientFactory,
        ContainerCredentialsConfiguration configuration,
        JsonSerializerOptions serializerOptions,
        ILogger<IAmazonClient>? serviceLogger) : BaseClient(nameof(IAmazonClient), clientFactory), IAmazonClient
    {
        private static readonly MediaTypeHeaderValue _contentType = new("application/x-amz-json-1.1");

        private ContainerCredentials? _credentials = default;
        private ContainerCredentialsProvider? _credentialsProvider = new(clientFactory, configuration, serializerOptions);

        public async Task<ContainerCredentials?> GetCredentialsAsync(CancellationToken cancellationToken)
        {
            if (_credentialsProvider == default)
            {
                return default;
            }

            if (_credentials == default || DateTime.Now > _credentials.Expiration)
            {
                if ((_credentials = await _credentialsProvider.GetCredentialsAsync(cancellationToken)) == default)
                {
                    // The only reason a provider can return null is if it is not possible to obtain
                    // credentials. So if the provider returns null, remove the provider so it does
                    // not try to obtain credentials next time and returns null immediately.
                    _credentialsProvider = default;
                }
            }

            return _credentials;
        }

        public async Task<IEnumerable<AmazonTask>> DescribeTasksAsync(
            IEnumerable<AmazonTaskArn> arns, CancellationToken cancellationToken)
        {
            var describedTasks = new List<AmazonTask>();
            foreach (var context in CreateRequestContexts(arns))
            {
                var credentials = await GetCredentialsAsync(cancellationToken);
                if (credentials == default)
                {
                    serviceLogger?.CannotDescribeECSTasks();
                    return [];
                }

                describedTasks.AddRange(await DescribeTasksAsync(context, credentials, cancellationToken));
            }

            static IEnumerable<RequestContext> CreateRequestContexts(IEnumerable<AmazonTaskArn> arns)
            {
                // To reduce the number of API requests to AWS, group the task ARNs by region and then
                // by cluster. In a standard configuration, only one group will exist, since the same
                // container instance can only be connected to one cluster. But this rule can be broken
                // by using a certain configuration.
                foreach (var regionGroup in arns.GroupBy(arn => arn.Region))
                {
                    foreach (var clusterGroup in regionGroup.GroupBy(arn => arn.Cluster))
                    {
                        // One API request may contain no more than 100 ARNs
                        foreach (var arnGroup in clusterGroup.Chunk(100))
                        {
                            yield return new RequestContext(regionGroup.Key)
                            {
                                Data = new AmazonTaskDescriptionRequest
                                {
                                    Cluster = clusterGroup.Key,
                                    Tasks = arnGroup,
                                }
                            };
                        }
                    }
                }
            }

            return describedTasks;
        }

        protected override HttpRequest CreateRequest(
            HttpMethod method, Uri requestUri, JsonSerializerOptions serializerOptions)
        {
            var request = base.CreateRequest(method, requestUri, serializerOptions);
            request.Message.Headers.Add(HeaderKeys.HostHeader, requestUri.Host);

            return request;
        }

        /// <summary>
        /// Sends an API request to AWS for describe ECS tasks.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <returns><see cref="Task{IEnumerable{AmazonTask}}" /> that completes with enumerate
        /// of the existed described tasks.</returns>
        private async Task<IEnumerable<AmazonTask>> DescribeTasksAsync(
            RequestContext context, ContainerCredentials credentials, CancellationToken cancellationToken)
        {
            var requestEndpoint = new Uri($"https://ecs.{context.Region}.amazonaws.com");

            using var request = CreateRequest(HttpMethod.Post, requestEndpoint, serializerOptions);
            using var requestContent = JsonContent.Create(context.Data, _contentType, serializerOptions);

            request.Message.Content = requestContent;
            var contentHash = await request.GetContentHashAsync(cancellationToken);

            var currentDate = DateTime.Now;
            var currentTimestamp = currentDate.ToString(AWSSDKUtils.ISO8601BasicDateTimeFormat);

            request.Message.Headers.Add(HeaderKeys.XAmzApiVersion, "2014-11-13");
            request.Message.Headers.Add(Headers.XAmzTarget, "AmazonEC2ContainerServiceV20141113.DescribeTasks");
            request.Message.Headers.Add(HeaderKeys.XAmzSecurityTokenHeader, credentials.Token);
            request.Message.Headers.Add(HeaderKeys.XAmzDateHeader, currentTimestamp);
            request.Message.Headers.Add(HeaderKeys.XAmzContentSha256Header, contentHash);

            var canonicalRequest = CanonicalRequest.Create(request, contentHash, "ecs");
            var signature = canonicalRequest.CreateSignature(currentDate, context, credentials, out var credentialScope);

            var authorizationHeader = string.Join(", ",
            [
                $"Credential={credentials.Id}/{credentialScope}",
                $"SignedHeaders={canonicalRequest.SignedHeaders}",
                $"Signature={signature}"
            ]);

            request.Message.Headers.Authorization = new AuthenticationHeaderValue(
                AWS4Signer.AWS4AlgorithmTag, authorizationHeader);

            var describedTasks = await request.ExecuteRequestAsync<AmazonTaskDescriptionResponse>(cancellationToken);
            if (describedTasks == default)
            {
                return [];
            }

            foreach (var describedTask in describedTasks.Tasks)
            {
                if (describedTask.Group.StartsWith("service:"))
                {
                    describedTask.Group = describedTask.Group["service:".Length..];
                }
            }

            return describedTasks.Tasks;
        }

        private class CanonicalRequest(string request, string signedHeaders, string service)
        {
            public string Request => request;
            public string SignedHeaders => signedHeaders;

            public static CanonicalRequest Create(HttpRequest request, string contentHash, string service)
            {
                var requestBuilder = new StringBuilder();
                requestBuilder.AppendLine(request.Message.Method.ToString());
                requestBuilder.AppendLine(request.Message.RequestUri?.AbsolutePath);
                requestBuilder.AppendLine();

                // Add the canonical headers, followed by a newline character. The canonical headers
                // consist of a list of all the HTTP headers that you are including with the signed
                // request. To create the canonical headers list, convert all header names to lowercase
                // and remove leading spaces and trailing spaces. Convert sequential spaces in the
                // header value to a single space.
                var sortedHeaders = request.GetSortedHeaders();
                foreach (var requestHeader in sortedHeaders)
                {
                    requestBuilder.AppendLine($"{requestHeader.Key}:{string.Join(", ", requestHeader.Value)}");
                }

                requestBuilder.AppendLine();

                // Add the signed headers, followed by a newline character. This value is the list of
                // headers that included in the canonical headers. By adding this list of headers, we
                // tell AWS which headers in the request are part of the signing process and which
                // ones AWS can ignore for purposes of validating the request.
                var signedHeaders = string.Join(";", sortedHeaders.Keys);
                requestBuilder.AppendLine(signedHeaders);

                requestBuilder.Append(contentHash);
                return new(requestBuilder.ToString(), signedHeaders, service);
            }

            public string CreateSignature(
                DateTime currentDate,
                RequestContext context,
                ContainerCredentials credentials,
                out string credentialScope)
            {
                credentialScope = string.Join('/',
                [
                    currentDate.ToString(AWSSDKUtils.ISO8601BasicDateFormat),
                    context.Region,
                    service,
                    AWS4Signer.Terminator
                ]);

                var contentBuilder = new StringBuilder()
                    .AppendLine(AWS4Signer.AWS4AlgorithmTag)
                    .AppendLine(currentDate.ToString(AWSSDKUtils.ISO8601BasicDateTimeFormat))
                    .AppendLine(credentialScope);

                var requestHash = AWSSDKUtils.ToHex(AWS4Signer.ComputeHash(request), true);
                contentBuilder.Append(requestHash);

                var signingKey = AWS4Signer.ComposeSigningKey(
                    credentials.Secret,
                    context.Region,
                    currentDate.ToString(AWSSDKUtils.ISO8601BasicDateFormat),
                    service);

                var signature = AWS4Signer.ComputeKeyedHash(
                    SigningAlgorithm.HmacSHA256, signingKey, contentBuilder.ToString());

                return AWSSDKUtils.ToHex(signature, true);
            }
        }

        private class RequestContext(string region)
        {
            public string Region => region;
            public required AmazonTaskDescriptionRequest Data { get; set; }
        }

        private class Headers
        {
            public const string XAmzTarget = "X-Amz-Target";
        }
    }
}
