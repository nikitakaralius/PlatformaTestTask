namespace PlatformaTestTask.Model;

internal sealed class Departure
{
    public int InitialStop { get; init; }

    public int FinalStop { get; init; }

    public TimeOnly StartTime { get; init; }
}
