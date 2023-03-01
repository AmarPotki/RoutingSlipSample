using MassTransit;

namespace RegistrationRoutingSlipSample.Api.Components.Activities;

public class ProcessPaymentActivity : IActivity<ProcessPaymentArguments, ProcessPaymentLog>
{
    private ILogger _logger;

    public ProcessPaymentActivity(ILogger<ProcessPaymentActivity> logger)
    {
        _logger = logger;
    }

    public async Task<ExecutionResult> Execute(ExecuteContext<ProcessPaymentArguments> context)
    {
        _logger.LogInformation("Processing Payment: {Amount}", context.Arguments.Amount);

        if (context.Arguments.CardNumber == "4147")
            throw new RoutingSlipException("The card number is invalid");

        if (context.Arguments.CardNumber == "187187")
            throw new TransientException("The payment provider isn't responding");

        if (context.Arguments.CardNumber == "187")
        {
            if (context.GetRetryAttempt() == 0 && context.GetRedeliveryCount() == 0)
                throw new TransientException("The payment provider isn't responding");

            if (context.GetRedeliveryCount() == 0)
                throw new LongTransientException("The payment provider isn't responding after a long time");
        }

        await Task.Delay(10);

        const string authorizationCode = "ABC123";

        return context.Completed(new
        {
            ChargeDate = DateTime.UtcNow,
            authorizationCode,
            context.Arguments.Amount,
        });
    }

    public async Task<CompensationResult> Compensate(CompensateContext<ProcessPaymentLog> context)
    {
        await Task.Delay(10);

        return context.Compensated();
    }
}

public class ProcessPaymentLog
{
    /// <summary>
    /// The date the charge was processed
    /// </summary>
    public DateTime ChargeDate { get; init; }

    /// <summary>
    /// The authorization code received from the payment provider
    /// </summary>
    public string AuthorizationCode { get; init; }

    /// <summary>
    /// The amount charged
    /// </summary>
    public decimal Amount { get; init; }
}

public class ProcessPaymentArguments
{
    public string CardNumber { get; init; }
    public string VerificationCode { get; init; }
    public string CardholderName { get; init; }
    public int ExpirationMonth { get; init; }
    public int ExpirationYear { get; init; }

    public decimal Amount { get; init; }
}

[Serializable]
public class LongTransientException :
    Exception
{
    public LongTransientException()
    {
    }

    public LongTransientException(string message)
        : base(message)
    {
    }
}
[Serializable]
public class TransientException :
    Exception
{
    public TransientException()
    {
    }

    public TransientException(string message)
        : base(message)
    {
    }
}