using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal sealed class CheapestRouteBuilder : IRouteBuilder
{
    private readonly RouteBuilder _routeBuilder;

    public CheapestRouteBuilder(IEnumerable<Transport> transport)
    {
        _routeBuilder = new RouteBuilder(transport, ChooseNearestTime);
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
