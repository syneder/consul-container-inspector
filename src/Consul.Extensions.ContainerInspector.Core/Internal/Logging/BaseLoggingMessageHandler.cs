namespace Consul.Extensions.ContainerInspector.Core.Internal.Logging
{
    internal abstract class BaseLoggingMessageHandler : DelegatingHandler
    {
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException(
                "Synchronous method Send is not supported. Use asynchronous method SendAsync instead of Send.");
        }
    }
}
