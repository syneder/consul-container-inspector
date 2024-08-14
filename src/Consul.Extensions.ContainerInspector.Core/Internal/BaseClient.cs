using Amazon.Util;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Provides basic methods for creating and executing Http requests.
    /// </summary>
    /// <param name="name">
    /// The name of the HTTP client that is created each time the <see cref="CreateRequest" /> method is called.
    /// </param>
    internal abstract class BaseClient(string name, IHttpClientFactory clientFactory)
    {
        protected virtual string BaseResourceURI { get; } = string.Empty;

        /// <summary>
        /// Creates new <see cref="HttpRequest" /> with specified <paramref name="method" />
        /// and <paramref name="resourceUri" />.
        /// </summary>
        /// <param name="resourceUri">A string representing the resource URI to which
        /// <see cref="BaseResourceUri" /> may be appended to create the request URI.</param>
        protected HttpRequest CreateRequest(
            HttpMethod method, string resourceUri, JsonSerializerOptions? serializerOptions = default)
        {
            var requestUri = string.Join('/', ((string[])[BaseResourceURI, resourceUri]).Except([string.Empty]));
            return CreateRequest(new HttpRequestMessage(method, '/' + requestUri.TrimStart('/')), serializerOptions);
        }

        /// <summary>
        /// Creates new <see cref="HttpRequest" /> with specified <paramref name="method" />
        /// and <paramref name="requestUri" />.
        /// </summary>
        protected virtual HttpRequest CreateRequest(
            HttpMethod method, Uri requestUri, JsonSerializerOptions? serializerOptions = default)
        {
            return CreateRequest(new HttpRequestMessage(method, requestUri), serializerOptions);
        }

        private HttpRequest CreateRequest(HttpRequestMessage requestMessage, JsonSerializerOptions? serializerOptions)
        {
            var requestMessageInvoker = clientFactory.CreateClient(name);
            return new(requestMessageInvoker, requestMessage, serializerOptions);
        }

        protected class HttpRequest(
            HttpClient messageInvoker, HttpRequestMessage message, JsonSerializerOptions? serializerOptions) : IDisposable
        {
            public HttpRequestMessage Message => message;

            public void Dispose()
            {
                message.Dispose();
                messageInvoker.Dispose();
            }

            /// <summary>
            /// Append the query keys and values to the request URI.
            /// </summary>
            /// <param name="queryParameters">A collection of name value query pairs to append.</param>
            public HttpRequest AddQueryParameters(Dictionary<string, string?> queryParameters)
            {
                var requestUri = (message.RequestUri?.ToString() ?? string.Empty).AsSpan();
                var requestUriSeparatorIndex = requestUri.IndexOf('?');
                var hasSeparator = requestUriSeparatorIndex >= 0;

                var requestUriBuilder = new StringBuilder().Append(requestUri);
                foreach (var queryParameter in queryParameters)
                {
                    if (queryParameter.Value == null)
                    {
                        continue;
                    }

                    requestUriBuilder.Append(hasSeparator ? '&' : '?');
                    requestUriBuilder.Append(UrlEncoder.Default.Encode(queryParameter.Key));
                    requestUriBuilder.Append('=');
                    requestUriBuilder.Append(UrlEncoder.Default.Encode(queryParameter.Value));

                    hasSeparator = true;
                }

                message.RequestUri = new(requestUriBuilder.ToString(), UriKind.RelativeOrAbsolute);
                return this;
            }

            /// <summary>
            /// Sends the request, waits to read the response headers, and ensures that the response
            /// status code is successful.
            /// </summary>
            public async Task<HttpResponseMessage> ExecuteRequestAsync(CancellationToken cancellationToken)
            {
                var responseMessage = await messageInvoker.SendAsync(
                    message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                return responseMessage.EnsureSuccessStatusCode();
            }

            /// <summary>
            /// Executes the request, reads the contents of the response, and deserializes it into an object.
            /// </summary>
            /// <param name="typeMetadata">Metadata about the type to deserialize.</param>
            public async Task<T?> ExecuteRequestAsync<T>(JsonTypeInfo<T> typeMetadata, CancellationToken cancellationToken)
            {
                using var responseMessage = await ExecuteRequestAsync(cancellationToken);
                using var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);

                return await JsonSerializer.DeserializeAsync(contentStream, typeMetadata, cancellationToken);
            }

            /// <summary>
            /// Executes the request, reads the contents of the response, and deserializes it into an object.
            /// </summary>
            public async Task<T?> ExecuteRequestAsync<T>(CancellationToken cancellationToken)
            {
                using var responseMessage = await ExecuteRequestAsync(cancellationToken);
                using var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);

                return await JsonSerializer.DeserializeAsync<T>(contentStream, serializerOptions, cancellationToken);
            }

            public async IAsyncEnumerable<string> GetStreamAsync(
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                using var responseMessage = await ExecuteRequestAsync(cancellationToken);
                using var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
                using var contentStreamReader = new StreamReader(contentStream, new UTF8Encoding(false));

                while (!cancellationToken.IsCancellationRequested)
                {
                    var content = await contentStreamReader.ReadLineAsync(cancellationToken);
                    if (content == default || cancellationToken.IsCancellationRequested)
                    {
                        continue;
                    }

                    yield return content;
                }
            }

            /// <summary>
            /// Returns the calculated SHA256 hash of the content.
            /// </summary>
            /// <remarks>
            /// If the request content is not specified, return the SHA256 hash of the empty string.
            /// </remarks>
            public async Task<string> GetContentHashAsync(CancellationToken cancellationToken)
            {
                if (message.Content == default)
                {
                    return AWSSDKUtils.ToHex(CryptoUtilFactory.CryptoInstance.ComputeSHA256Hash([]), true);
                }

                var contentStream = await message.Content.ReadAsStreamAsync(cancellationToken);
                var contentPosition = contentStream.Position;

                var contentHash = CryptoUtilFactory.CryptoInstance.ComputeSHA256Hash(contentStream);
                contentStream.Position = contentPosition;

                return AWSSDKUtils.ToHex(contentHash, true);
            }

            public SortedDictionary<string, List<string>> GetSortedHeaders()
            {
                var sortedHeaders = new SortedDictionary<string, List<string>>();
                foreach (var requestHeader in message.Headers)
                {
                    AppendHeader(requestHeader.Key.ToLowerInvariant(), requestHeader.Value);
                }

                foreach (var requestHeader in messageInvoker.DefaultRequestHeaders)
                {
                    if (!sortedHeaders.ContainsKey(requestHeader.Key.ToLowerInvariant()))
                    {
                        AppendHeader(requestHeader.Key.ToLowerInvariant(), requestHeader.Value);
                    }
                }

                return sortedHeaders;

                void AppendHeader(string name, IEnumerable<string> values)
                {
                    if (!sortedHeaders.TryGetValue(name, out var existedValues))
                    {
                        sortedHeaders.Add(name, existedValues = []);
                    }

                    existedValues.AddRange(values.Select(value => value.Trim()));
                }
            }
        }
    }
}
