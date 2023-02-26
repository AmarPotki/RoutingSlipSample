namespace RoutingSlipSample.Contracts;

public record FryShake
{
    public Guid FryShakeId { get; init; }
    public string Flavor { get; init; }
    public Size Size { get; init; }

    public override string ToString()
    {
        return $"{Size} {Flavor} FryShake";
    }
}