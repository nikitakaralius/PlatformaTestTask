using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal sealed class CheapestRouteBuilder : IRouteBuilder
{
    private readonly RouteBuilder<int> _routeBuilder;

    public CheapestRouteBuilder(IEnumerable<Transport> transport)
    {
        _routeBuilder = new RouteBuilder<int>(transport, ChooseNearestTime, cost => cost.AccumulativeCost);
    }

    public Route Build(Departure departure)
    {
        return _routeBuilder.Build(departure);
    }

    private int ChooseNearestTime(TransitionCost previous, TransitionCost next)
    {
        return previous.AccumulativeCost.CompareTo(next.AccumulativeCost);
    }
}
