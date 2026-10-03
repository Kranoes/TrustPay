using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TrustPay.Application.Common.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
        private const int SlowRequestThresholdMs = 500;

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();

            var response = await next();

            timer.Stop();

            var elapsedMilliseconds = timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > SlowRequestThresholdMs)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogWarning(
                    "[ALARM] МЕДЛЕННЫЙ ЗАПРОС: {RequestName} выполнялся {ElapsedMilliseconds} мс (Порог: {Threshold} мс)",
                    requestName, elapsedMilliseconds, SlowRequestThresholdMs);
            }

            return response;
        }
    }
}