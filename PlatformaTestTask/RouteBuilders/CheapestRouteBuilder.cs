using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal sealed class CheapestRouteBuilder : RouteBuilder
{
    public CheapestRouteBuilder(IEnumerable<Transport> transport) : base(transport) { }

    public override Route Build(Departure departure)
    {
        return Build(departure, (previousCost, newCost) =>
        {
            return previousCost.MoneyToArrive - newCost.MoneyToArrive;
        });
    }

    protected override RouteNodeId? FindBestSuitableNode(
        Dictionary<RouteNodeId, TransitionCost> costs,
        ISet<RouteNodeId> processed
    )
    {
        int lowestCost = int.MaxValue;
        RouteNodeId? lowestCostNode = null;

        foreach (var node in costs.Keys)
        {
            int cost = costs[node].MoneyToArrive;

            if (cost < lowestCost && processed.Contains(node) == false)
            {
                lowestCost = cost;
                lowestCostNode = node;
            }
        }

        return lowestCostNode;
    }
}
