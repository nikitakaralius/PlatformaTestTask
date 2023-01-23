using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal sealed class FastestRouteBuilder : RouteBuilder
{
    public FastestRouteBuilder(IEnumerable<Transport> transport) : base(transport) { }

    public override Route Build(Departure departure)
    {
        return Build(departure,
            (previousCost, newCost) =>
            {
                return previousCost.TimeToArrive.CompareTo(newCost.TimeToArrive);
            },
            (transitionDelegate, costs) =>
            {
                var key = transitionDelegate.MinByArrivalTime(departure.FinalStop);
                var value = costs[key];

                return new RouteNode
                {
                    Id = key,
                    TransitionCost = value
                };
            });
    }

    protected override RouteNodeId? FindBestSuitableNode(
        Dictionary<RouteNodeId, TransitionCost> costs,
        ISet<RouteNodeId> processed
    )
    {
        var nearestArrival = TimeSpan.MaxValue;
        RouteNodeId? nearestArrivalNode = null;

        foreach (var node in costs.Keys)
        {
            var cost = costs[node].TimeToArrive;

            if (cost < nearestArrival && processed.Contains(node) == false)
            {
                nearestArrival = cost;
                nearestArrivalNode = node;
            }
        }

        return nearestArrivalNode;
    }
}
