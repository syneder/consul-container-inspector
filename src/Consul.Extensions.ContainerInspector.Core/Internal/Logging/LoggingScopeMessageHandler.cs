using Consul.Extensions.ContainerInspector.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Consul.Extensions.ContainerInspector.Core.Internal.Logging
{
    internal class LoggingScopeMessageHandler(ILogger serviceLogger) : BaseLoggingMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            using (serviceLogger.CreateRequestScope(request))
            {
                serviceLogger.RequestScopeCreated(request);
                var response = await base.SendAsync(request, cancellationToken);

                serviceLogger.RequestScopeCompleted(response, stopwatch);
                return response;
            }
        }
    }
}
