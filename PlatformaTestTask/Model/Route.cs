using System.Text;

namespace PlatformaTestTask.Model;

internal sealed class Route
{
    private readonly IEnumerable<IRouteNode> _nodes;

    public Route(IEnumerable<IRouteNode> nodes)
    {
        _nodes = nodes;
    }

    public static readonly Route Empty = new(Array.Empty<IRouteNode>());

    public int MoneySpent => _nodes.Sum(n => n.TransitionCost.MoneyToArrive);

    public TimeSpan TimeSpent => _nodes.Aggregate(TimeSpan.Zero, (span, node) => span + node.TransitionCost.TimeToArrive);

    public override string ToString()
    {
        if (_nodes.Any() == false)
        {
            return "Unable to build such route";
        }

        var sb = new StringBuilder();
        int currentBus = -1;

        foreach (var routeNode in _nodes)
        {
            if (routeNode.Id.BusNumber != currentBus)
            {
                currentBus = routeNode.Id.BusNumber;
                sb.AppendLine($"Take a seat on {currentBus} bus. It will cost you {routeNode.TransitionCost.MoneyToArrive}");
            }

            sb.Append($"You're at stop number {routeNode.Id.StopNumber}. ");
            sb.AppendLine($"It will take you {routeNode.TransitionCost.TimeToArrive} to get to the next stop");
        }

        sb.AppendLine();
        sb.AppendLine($"Time Spent: {TimeSpent}, Money Spent: {MoneySpent}");

        return sb.ToString();
    }
}
