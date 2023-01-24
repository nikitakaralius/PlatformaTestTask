namespace PlatformaTestTask.Model;

internal sealed record TransitionCost
{
    public int AccumulativeCost { get; set; }

    public TimeOnly AccumulativeTime { get; set; }

    public static readonly TransitionCost Max = new()
    {
        AccumulativeCost = int.MaxValue,
        AccumulativeTime = TimeOnly.MaxValue
    };
}
