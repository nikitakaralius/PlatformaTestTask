namespace PlatformaTestTask.Model;

internal sealed record TransitionCost
{
    public TimeSpan TimeToArrive { get; set; }

    public int MoneyToArrive { get; set; }

    public static readonly TransitionCost Max = new()
    {
        TimeToArrive = TimeSpan.MaxValue,
        MoneyToArrive = int.MaxValue
    };
}
