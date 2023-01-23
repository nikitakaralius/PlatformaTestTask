using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal abstract class RouteBuilder
{
    private readonly IEnumerable<Transport> _transport;

    protected RouteBuilder(IEnumerable<Transport> transport)
    {
        _transport = transport;
    }

    public abstract Route Build(Departure departure);

    protected Route Build(Departure departure, Func<TransitionCost, TransitionCost, int> comparer)
    {
        var transitionDelegate = new TimeTransitionDelegate(_transport, departure);
        var graph = RouteGraph.Create(_transport).AddTransitions(departure.InitialStop, transitionDelegate);

        var costs = CreateCosts(graph, transitionDelegate);
        var parents = CreateParents(graph);
        var processed = new HashSet<RouteNodeId>();

        RouteNodeId? node = FindBestSuitableNode(costs, processed);

        if (node is null)
        {
            return Route.Empty;
        }

        while (node is not null)
        {
            var cost = costs[node];
            var destinations = graph.Nodes[node];

            foreach (var destination in destinations)
            {
                int newMoneyToArrive = cost.MoneyToArrive + destination.TransitionCost.MoneyToArrive;
                var newTimeToArrive = destination.TransitionCost.TimeToArrive;

                var newCost = new TransitionCost
                {
                    MoneyToArrive = newMoneyToArrive,
                    TimeToArrive = newTimeToArrive
                };

                if (comparer(costs[destination.Id], newCost) > 0)
                {
                    costs[destination.Id] = destination.TransitionCost with {MoneyToArrive = newMoneyToArrive};
                    parents[destination.Id] = node;
                    transitionDelegate.UpdateArrival(node, destination.Id, destination.TransitionCost);
                }
            }

            processed.Add(node);
            node = FindBestSuitableNode(costs, processed);
        }

        return new Route(BuildFormattedRoute(costs, parents, () =>
        {
            var key = transitionDelegate.MinByArrivalTime(departure.FinalStop);
            var value = costs[key];

            return new RouteNode
            {
                Id = key,
                TransitionCost = value
            };
        }));
    }

    protected abstract RouteNodeId? FindBestSuitableNode(
        Dictionary<RouteNodeId, TransitionCost> costs,
        ISet<RouteNodeId> processed
    );

    private static IEnumerable<IRouteNode> BuildFormattedRoute(
        Dictionary<RouteNodeId, TransitionCost> costs,
        Dictionary<RouteNodeId, RouteNodeId?> parents,
        Func<RouteNode> finalNode
    )
    {
        var node = finalNode();
        var id = node.Id;
        var transitionCost = node.TransitionCost;

        List<IRouteNode> route = new()
        {
            new RouteNode
            {
                Id = id,
                TransitionCost = transitionCost
            }
        };

        while (parents[id!] != RouteNodeId.Initial)
        {
            id = parents[id!];
            var cost = costs[id!];

            var routeNode = new RouteNode
            {
                Id = id!,
                TransitionCost = cost
            };

            route.Add(routeNode);
        }

        route.Reverse();

        int previousCost = route[0].TransitionCost.MoneyToArrive;

        for (int i = 1; i < route.Count; i++)
        {
            int intermediateValue = route[i].TransitionCost.MoneyToArrive - previousCost;
            previousCost = route[i].TransitionCost.MoneyToArrive;
            route[i].TransitionCost.MoneyToArrive = intermediateValue;
        }

        return route;
    }

    private static Dictionary<RouteNodeId, TransitionCost> CreateCosts(
        RouteGraph graph,
        TimeTransitionDelegate transitionDelegate
    )
    {
        var costs = new Dictionary<RouteNodeId, TransitionCost>();

        foreach (var node in graph.Nodes[RouteNodeId.Initial])
        {
            costs.Add(node.Id, node.TransitionCost);

            transitionDelegate.UpdateArrival(
                RouteNodeId.Initial, node.Id, transitionDelegate.CalculateTransitionCost(RouteNodeId.Initial, node.Id));
        }

        foreach (var nodeId in graph.Nodes.Keys.Except(costs.Keys))
        {
            if (nodeId == RouteNodeId.Initial) continue;

            costs.Add(nodeId, TransitionCost.Max);
        }

        return costs;
    }

    private static Dictionary<RouteNodeId, RouteNodeId?> CreateParents(RouteGraph graph)
    {
        var parents = new Dictionary<RouteNodeId, RouteNodeId?>();

        foreach (var parent in graph.Nodes.Keys)
        {
            foreach (var child in graph.Nodes[parent])
            {
                if (parents.ContainsKey(child.Id)) continue;

                parents.Add(child.Id, null);
            }
        }

        foreach (var node in graph.Nodes[RouteNodeId.Initial])
        {
            if (parents.ContainsKey(node.Id))
            {
                parents[node.Id] = RouteNodeId.Initial;
            }
            else
            {
                parents.Add(node.Id, RouteNodeId.Initial);
            }
        }

        return parents;
    }
}
