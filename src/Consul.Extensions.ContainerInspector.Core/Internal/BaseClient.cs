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
        protected HttpRequest CreateRequest(HttpMethod method, string resourceUri)
        {
            var requestUri = string.Join('/', ((string[])[BaseResourceURI, resourceUri]).Except([string.Empty]));
            var requestMessageInvoker = clientFactory.CreateClient(name);
            return new(requestMessageInvoker, new HttpRequestMessage(method, '/' + requestUri));
        }

        protected class HttpRequest(HttpMessageInvoker messageInvoker, HttpRequestMessage message) : IDisposable
        {
            public HttpRequestMessage RequestMessage => message;

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

                message.RequestUri = new(requestUriBuilder.ToString());
                return this;
            }

            /// <summary>
            /// Sends the request, waits to read the response headers, and ensures that the response
            /// status code is successful.
            /// </summary>
            public async Task<HttpResponseMessage> ExecuteRequestAsync(CancellationToken cancellationToken)
            {
                var responseMessage = await messageInvoker.SendAsync(message, cancellationToken);
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

                return await JsonSerializer.DeserializeAsync<T>(contentStream, typeMetadata, cancellationToken);
            }

            public async IAsyncEnumerable<T> GetStreamAsync<T>(
                JsonTypeInfo<T> typeMetadata, [EnumeratorCancellation] CancellationToken cancellationToken) where T : class
            {
                using var responseMessage = await ExecuteRequestAsync(cancellationToken);
                using var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
                using var contentStreamReader = new StreamReader(contentStream, new UTF8Encoding(false));

                while (!cancellationToken.IsCancellationRequested)
                {
                    var responseContent = await contentStreamReader.ReadLineAsync(cancellationToken);
                    if (responseContent == default || cancellationToken.IsCancellationRequested)
                    {
                        continue;
                    }

                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(responseContent));
                    var response = await JsonSerializer.DeserializeAsync<T>(stream, typeMetadata, cancellationToken);
                    if (response == default)
                    {
                        continue;
                    }

                    yield return response;
                }
            }
        }
    }
}
