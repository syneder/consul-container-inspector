using Consul.Extensions.ContainerInspector.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Consul.Extensions.ContainerInspector.Core.Internal.Logging
{
    internal class LoggingScopeMessageHandler(ILogger serviceLogger) : BaseLoggingMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            using (serviceLogger.CreateRequestScope(requestMessage))
            {
                serviceLogger.RequestScopeCreated(requestMessage);
                var responseMessage = await base.SendAsync(requestMessage, cancellationToken);

                serviceLogger.RequestScopeCompleted(responseMessage, stopwatch);
                return responseMessage;
            }
        }
    }
}
