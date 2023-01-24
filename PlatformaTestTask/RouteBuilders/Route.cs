using System.Text;
using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal sealed class Route
{
    private readonly IEnumerable<(RouteNode, TransitionCost)> _route;

    public Route(IEnumerable<(RouteNode, TransitionCost)> route)
    {
        _route = route;
    }

    public TimeSpan TotalTime => _route.Last().Item2.AccumulativeTime - _route.First().Item2.AccumulativeTime;

    public int TotalMoney => _route.Last().Item2.AccumulativeCost;

    public string Verbose()
    {
        if (_route.Any() == false)
        {
            return "Cannot find such route";
        }

        var sb = new StringBuilder();
        int currentBus = -1;

        foreach (var (node, transitionCost) in _route)
        {
            if (node.BusNumber != currentBus)
            {
                currentBus = node.BusNumber;
                sb.AppendLine($"Take a seat on {currentBus} bus.");
            }

            sb.AppendLine($"You're at stop number {node.StopNumber}. The bus arrives - {transitionCost.AccumulativeTime}");
        }

        sb.AppendLine();
        sb.AppendLine($"Total money spent: {TotalMoney}");
        sb.AppendLine($"Total time spent: {TotalTime}");

        return sb.ToString();
    }
}
