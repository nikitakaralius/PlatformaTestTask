using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal sealed class RouteGraph
{
    private readonly Dictionary<RouteNodeId, List<IRouteNode>> _graph;

    private RouteGraph(Dictionary<RouteNodeId, List<IRouteNode>> graph)
    {
        _graph = graph;
    }

    public IReadOnlyDictionary<RouteNodeId, List<IRouteNode>> Nodes => _graph;

    public static RouteGraph Create(IEnumerable<Transport> transport)
    {
        var graph = new Dictionary<RouteNodeId, List<IRouteNode>>();

        void AddGraphEdge(int previous, int current, Transport route)
        {
            var previousId = new RouteNodeId(route.BusNumber, route.StopNumbers[previous]);

            graph.Add(previousId, new List<IRouteNode>
            {
                new RouteNode
                {
                    Id = new RouteNodeId(route.BusNumber, route.StopNumbers[current]),
                    TransitionCost = new TransitionCost
                    {
                        TimeToArrive = TimeSpan.FromMinutes(route.TimeBetweenStops[previous]),
                        MoneyToArrive = 0
                    }
                }
            });
        }

        foreach (var route in transport)
        {
            for (int i = 1; i < route.StopNumbers.Length; i++)
            {
                AddGraphEdge(i - 1, i, route);
            }

            AddGraphEdge(route.StopNumbers.Length - 1, 0, route);
        }

        return new RouteGraph(graph);
    }

    public RouteGraph AddTransitions(int initialStop, ITransitionDelegate transitionDelegate)
    {
        BreakLoopOn(initialStop);

        _graph[RouteNodeId.Initial] = new List<IRouteNode>();

        AddTransitionFromInitialNode(initialStop, transitionDelegate);

        AddTransitionsBetweenStops(transitionDelegate);

        return this;
    }

    private void BreakLoopOn(int initialStop)
    {
        foreach ((_, List<IRouteNode> value) in _graph)
        {
            value.RemoveAll(node => node.Id.StopNumber == initialStop);
        }
    }

    private void AddTransitionsBetweenStops(ITransitionDelegate transitionDelegate)
    {
        foreach (var id1 in _graph.Keys)
        {
            foreach (var id2 in _graph.Keys)
            {
                if (id1 == id2) continue;

                if (id1.StopNumber != id2.StopNumber) continue;

                _graph[id1].Add(new TransportChangeRouteNode
                {
                    Id = id2,
                    From = id1,
                    Delegate = transitionDelegate
                });
            }
        }
    }

    private void AddTransitionFromInitialNode(int initialStop, ITransitionDelegate transitionDelegate)
    {
        foreach (var id in _graph.Keys)
        {
            if (id.StopNumber == initialStop)
            {
                _graph[RouteNodeId.Initial].Add(new TransportChangeRouteNode
                {
                    Id = id,
                    From = RouteNodeId.Initial,
                    Delegate = transitionDelegate
                });
            }
        }
    }
}
