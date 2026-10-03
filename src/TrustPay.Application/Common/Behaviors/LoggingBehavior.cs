using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TrustPay.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var timer = System.Diagnostics.Stopwatch.StartNew();

            _logger.LogInformation("Старт обработки запроса: {RequestName} ", requestName);

            var response = await next();
            timer.Stop();
            if (timer.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning("Обработка запроса {RequestName} заняла {ElapsedMilliseconds} мс", requestName, timer.ElapsedMilliseconds);
            }
            else 
            {
            _logger.LogInformation("Запрос {RequestName} успешно обработан за {ElapsedMilliseconds} мс", requestName, timer.ElapsedMilliseconds);
            }

            return response;
        }
    }
}