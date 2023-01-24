using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal sealed class RouteBuilder<TOptimize> : IRouteBuilder
{
    private readonly IEnumerable<Transport> _transport;
    private readonly Func<TransitionCost, TransitionCost, int> _comparer;
    private readonly Func<TransitionCost, TOptimize> _finalElementSelector;

    public RouteBuilder(
        IEnumerable<Transport> transport,
        Func<TransitionCost, TransitionCost, int> comparer,
        Func<TransitionCost, TOptimize> finalElementSelector)
    {
        _transport = transport;
        _comparer = comparer;
        _finalElementSelector = finalElementSelector;
    }

    public Route Build(Departure departure)
    {
        var graph = RouteGraph.Create(departure.InitialStop, _transport);
        var costs = CreateCosts(departure, graph);
        var parents = CreateParents(graph);
        var processed = new HashSet<RouteNode>();

        RouteNode? currentPosition = FindNearestArrivalNode(costs, processed);

        while (currentPosition is not null)
        {
            var accumulatedCost = costs[currentPosition];
            var destinations = graph.Nodes[currentPosition];

            foreach (var destination in destinations)
            {
                var costToArrive = CostToArrive(currentPosition, destination, accumulatedCost);

                if (_comparer(costs[destination], costToArrive) > 0)
                {
                    costs[destination] = costToArrive;
                    parents[destination] = currentPosition;
                }
            }

            processed.Add(currentPosition);
            currentPosition = FindNearestArrivalNode(costs, processed);
        }

        return new Route(TrackFastestRoute(departure, costs, parents));
    }

    private IEnumerable<(RouteNode, TransitionCost)> TrackFastestRoute(Departure departure, Dictionary<RouteNode, TransitionCost> costs, Dictionary<RouteNode, RouteNode?> parents)
    {
        var (rootNode, transitionCost) = costs.Where(kv => kv.Key.StopNumber == departure.FinalStop)
                                              .MinBy(kv => _finalElementSelector(kv.Value));

        var route = new List<(RouteNode, TransitionCost)>();

        do
        {
            if (rootNode is null)
            {
                return Array.Empty<(RouteNode, TransitionCost)>();
            }

            route.Add((rootNode, transitionCost));
            rootNode = parents[rootNode]!;
            transitionCost = costs[rootNode];
        }
        while (rootNode != RouteNode.Initial);

        route.Reverse();

        return route;
    }

    private TransitionCost CostToArrive(RouteNode from, RouteNode to, TransitionCost accumulatedCost)
    {
        var toRoute = _transport.First(t => t.BusNumber == to.BusNumber);

        int transitionCost = from.BusNumber == to.BusNumber ? 0 : toRoute.Cost;

        return new TransitionCost
        {
            AccumulativeCost = accumulatedCost.AccumulativeCost + transitionCost,
            AccumulativeTime = NearestArrival(to, accumulatedCost.AccumulativeTime)
        };
    }

    private TimeOnly NearestArrival(RouteNode to, TimeOnly currentTime)
    {
        int[] PrefixSum(Transport transport)
        {
            int[] prefixSum = new int[transport.StopNumbers.Length + 1];

            for (int i = 1; i < prefixSum.Length; i++)
                prefixSum[i] = transport.TimeBetweenStops[i - 1] + prefixSum[i - 1];

            return prefixSum;
        }

        var toRoute = _transport.First(t => t.BusNumber == to.BusNumber);

        int[] toPrefixSum = PrefixSum(toRoute);
        int stopIndex = toRoute.StopNumbers.ToList().IndexOf(to.StopNumber);

        if (currentTime.AddMinutes(toPrefixSum[stopIndex]) <= toRoute.StartsWorkingAt)
        {
            return toRoute.StartsWorkingAt;
        }

        var timePassedSinceStarted = currentTime - toRoute.StartsWorkingAt;
        var loopsPassed = timePassedSinceStarted.TotalMinutes / toPrefixSum[^1];

        var lesserTime =
            toRoute.StartsWorkingAt.AddMinutes((int) loopsPassed * toPrefixSum[^1] + toPrefixSum[stopIndex]);

        var greaterTime =
            toRoute.StartsWorkingAt.AddMinutes((int) Math.Ceiling(loopsPassed) * toPrefixSum[^1] +
                                               toPrefixSum[stopIndex]);

        return currentTime <= lesserTime ? lesserTime : greaterTime;
    }

    private static Dictionary<RouteNode, TransitionCost> CreateCosts(Departure departure, RouteGraph graph)
    {
        var costs = graph.Nodes.Keys.ToDictionary(node => node, _ => TransitionCost.Max);

        costs[RouteNode.Initial] = new TransitionCost
        {
            AccumulativeCost = 0,
            AccumulativeTime = departure.StartTime
        };

        return costs;
    }

    private static Dictionary<RouteNode, RouteNode?> CreateParents(RouteGraph graph)
    {
        var parents = new Dictionary<RouteNode, RouteNode?>();

        foreach (var node in graph.Nodes.Keys)
        {
            parents.Add(node, null);
        }

        foreach (var child in graph.Nodes[RouteNode.Initial])
        {
            parents[child] = RouteNode.Initial;
        }

        return parents;
    }

    private RouteNode? FindNearestArrivalNode(
        Dictionary<RouteNode, TransitionCost> costs,
        HashSet<RouteNode> processed
    )
    {
        var nearestArrival = TransitionCost.Max;
        RouteNode? nearestArrivalNode = null;

        foreach (var (node, cost) in costs)
        {
            if (_comparer(nearestArrival, cost) > 0 && processed.Contains(node) == false)
            {
                nearestArrival = cost;
                nearestArrivalNode = node;
            }
        }

        return nearestArrivalNode;
    }
}
