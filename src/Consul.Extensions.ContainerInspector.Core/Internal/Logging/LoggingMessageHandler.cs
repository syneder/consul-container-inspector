using Consul.Extensions.ContainerInspector.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Consul.Extensions.ContainerInspector.Core.Internal.Logging
{
    internal class LoggingMessageHandler(ILogger serviceLogger) : BaseLoggingMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            // Not using a scope here because we always expect this to be at the end of the pipeline, thus there's
            // not really anything to surround.
            serviceLogger.RequestCreated();
            var responseMessage = await base.SendAsync(requestMessage, cancellationToken);

            serviceLogger.RequestCompleted(stopwatch.ElapsedMilliseconds, responseMessage.StatusCode);
            return responseMessage;
        }
    }
}
