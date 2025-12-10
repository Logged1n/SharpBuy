using Application.Abstractions.Payments;
using Microsoft.Extensions.Configuration;
using SharedKernel;
using Stripe;

namespace Infrastructure.Payments;

internal sealed class StripePaymentService : IPaymentService
{
    public StripePaymentService(IConfiguration configuration)
    {
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
    }

    public async Task<Result<string>> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string customerEmail,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = currency.ToUpperInvariant(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                ReceiptEmail = customerEmail,
            };

            var service = new PaymentIntentService();
            PaymentIntent paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return Result.Success(paymentIntent.ClientSecret);
        }
        catch (StripeException ex)
        {
            return Result.Failure<string>(Error.Failure(
                "Payment.StripeError",
                $"Stripe error: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Failure(
                "Payment.Error",
                $"Payment error: {ex.Message}"));
        }
    }

    public async Task<Result> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            PaymentIntent paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

            if (paymentIntent.Status != "succeeded")
            {
                return Result.Failure(Error.Problem(
                    "Payment.NotSucceeded",
                    "Payment was not successful"));
            }

            return Result.Success();
        }
        catch (StripeException ex)
        {
            return Result.Failure(Error.Failure(
                "Payment.StripeError",
                $"Stripe error: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure(
                "Payment.Error",
                $"Payment error: {ex.Message}"));
        }
    }

    public async Task<Result> RefundPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
            };

            var service = new RefundService();
            await service.CreateAsync(options, cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (StripeException ex)
        {
            return Result.Failure(Error.Failure(
                "Payment.StripeError",
                $"Stripe error: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure(
                "Payment.Error",
                $"Payment error: {ex.Message}"));
        }
    }
}
