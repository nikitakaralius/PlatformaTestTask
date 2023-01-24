using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal sealed class FastestRouteBuilder : IRouteBuilder
{
    private readonly RouteBuilder<TimeOnly> _routeBuilder;

    public FastestRouteBuilder(IEnumerable<Transport> transport)
    {
        _routeBuilder = new RouteBuilder<TimeOnly>(transport, ChooseNearestTime, cost => cost.AccumulativeTime);
    }

    public Route Build(Departure departure)
    {
        return _routeBuilder.Build(departure);
    }

    private int ChooseNearestTime(TransitionCost previous, TransitionCost next)
    {
        return previous.AccumulativeTime.CompareTo(next.AccumulativeTime);
    }
}
