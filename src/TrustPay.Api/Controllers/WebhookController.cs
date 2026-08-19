using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Api.Filters;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Webhook;
using TrustPay.Application.Common.Transactions.Commands.ProcessBankWebhook;

namespace TrustPay.Api.Controllers;

[Route("api/v1/webhooks")]
[EnableBuffering]
public class WebhookController : ApiController
{
    private readonly IPaymentSignatureValidator _signatureValidator;

    public WebhookController(IPaymentSignatureValidator signatureValidator)
    {
        _signatureValidator = signatureValidator;
    }

    [HttpPost("bank")]
    public async Task<IActionResult> ProcessBankWebhook(
        [FromBody] BankWebhookRequest request,
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
        {
            return BadRequest("Отсутствует заголовок подписи.");
        }

        Request.EnableBuffering();
        Request.Body.Position = 0;

        using var reader = new StreamReader(
            Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var isValid = _signatureValidator.Validate(rawBody, signatureHeader.ToString());
        if (!isValid)
        {
            return Unauthorized("Недействительная подпись вебхука.");
        }

        var command = new ProcessBankWebhookCommand(
            request.TransactionId,
            request.IsSuccess,
            request.FailureReason,
            request.ExternalPaymentId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}
public record BankWebhookRequest(
          Guid TransactionId,
          bool IsSuccess,
          string? FailureReason,
          string? ExternalPaymentId);

