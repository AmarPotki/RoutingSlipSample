namespace RoutingSlipSample.Contracts;

public record Shake
{
    public Guid ShakeId { get; init; }
    public string Flavor { get; init; }
    public Size Size { get; init; }

    public override string ToString()
    {
        return $"{Size} {Flavor} Shake";
    }
}