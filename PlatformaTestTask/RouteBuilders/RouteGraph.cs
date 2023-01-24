using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal sealed record RouteNode(int BusNumber, int StopNumber)
{
    public static readonly RouteNode Initial = new(-1, -1);
}

internal sealed class RouteGraph
{
    private readonly Dictionary<RouteNode, List<RouteNode>> _graph;

    private RouteGraph(Dictionary<RouteNode, List<RouteNode>> graph)
    {
        _graph = graph;
    }

    public IReadOnlyDictionary<RouteNode, List<RouteNode>> Nodes => _graph;

    public static RouteGraph Create(int initialStop, IEnumerable<Transport> transport)
    {
        var graph = new Dictionary<RouteNode, List<RouteNode>>();

        AddGraphEdgesPerEachRoute(transport, graph);
        BreakLoopAtStartStop(initialStop, graph);
        AddEdgesFromInitialNode(initialStop, graph);
        AddEdgesBetweenDifferentRoutes(graph);

        return new RouteGraph(graph);
    }

    private static void AddGraphEdgesPerEachRoute(IEnumerable<Transport> transport, Dictionary<RouteNode, List<RouteNode>> graph)
    {
        void AddGraphEdge(int previous, int current, Transport route)
        {
            var previousNode = new RouteNode(route.BusNumber, route.StopNumbers[previous]);

            graph.Add(previousNode, new List<RouteNode>
            {
                new(route.BusNumber, route.StopNumbers[current])
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
    }

    private static void BreakLoopAtStartStop(int startStop, Dictionary<RouteNode, List<RouteNode>> graph)
    {
        foreach ((_, List<RouteNode> children) in graph)
        {
            children.RemoveAll(child => child.StopNumber == startStop);
        }
    }

    private static void AddEdgesBetweenDifferentRoutes(Dictionary<RouteNode, List<RouteNode>> graph)
    {
        foreach (var node1 in graph.Keys)
        {
            foreach (var node2 in graph.Keys)
            {
                if (node1 == node2)
                    continue;

                if (node1.StopNumber != node2.StopNumber)
                    continue;

                graph[node1].Add(node2);
            }
        }
    }

    private static void AddEdgesFromInitialNode(int initialStop, Dictionary<RouteNode, List<RouteNode>> graph)
    {
        graph[RouteNode.Initial] = new List<RouteNode>();

        foreach (var node in graph.Keys)
        {
            if (node.StopNumber == initialStop)
            {
                graph[RouteNode.Initial].Add(node);
            }
        }
    }
}
