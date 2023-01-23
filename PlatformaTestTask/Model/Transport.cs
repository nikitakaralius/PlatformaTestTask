namespace PlatformaTestTask.Model;

internal sealed class Transport
{
    public int BusNumber { get; init; }

    public int[] StopNumbers { get; init; } = Array.Empty<int>();

    public int[] TimeBetweenStops { get; init; } = Array.Empty<int>();

    public TimeOnly StartsWorkingAt { get; init; }

    public int Cost { get; init; }
}
