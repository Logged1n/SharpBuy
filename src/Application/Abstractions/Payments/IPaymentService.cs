using SharedKernel;

namespace Application.Abstractions.Payments;

public interface IPaymentService
{
    Task<Result<string>> CreatePaymentIntentAsync(decimal amount, string currency, string customerEmail, CancellationToken cancellationToken = default);
    Task<Result> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<Result> RefundPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
}
