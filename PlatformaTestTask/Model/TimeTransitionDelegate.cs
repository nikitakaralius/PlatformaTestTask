using PlatformaTestTask.RouteBuilders;

namespace PlatformaTestTask.Model;

internal sealed class TimeTransitionDelegate : ITransitionDelegate
{
    private readonly IEnumerable<Transport> _transport;
    private readonly Dictionary<RouteNodeId, TimeOnly> _timeWhenArrived;

    public TimeTransitionDelegate(IEnumerable<Transport> transport, Departure departure)
    {
        _transport = transport;

        _timeWhenArrived = new Dictionary<RouteNodeId, TimeOnly>
        {
            [RouteNodeId.Initial] = departure.StartTime
        };
    }

    public TransitionCost CalculateTransitionCost(RouteNodeId from, RouteNodeId to)
    {
        var currentTime = _timeWhenArrived[from];
        var transport = _transport.First(t => t.BusNumber == to.BusNumber);
        var nearestArrival = NearestArrival(transport, to.StopNumber, currentTime);

        return new TransitionCost
        {
            MoneyToArrive = from.BusNumber == to.BusNumber ? 0 : transport.Cost,
            TimeToArrive = nearestArrival
        };
    }

    public void UpdateArrival(RouteNodeId from, RouteNodeId to, TransitionCost cost)
    {
        var time = _timeWhenArrived[from].Add(cost.TimeToArrive);

        if (_timeWhenArrived.ContainsKey(to))
        {
            _timeWhenArrived[to] = time;
        }
        else
        {
            _timeWhenArrived.Add(to, time);
        }
    }

    public RouteNodeId MinByArrivalTime(int stopNumber)
    {
        return _timeWhenArrived.Where(kv => kv.Key.StopNumber == stopNumber).MinBy(kv => kv.Value).Key;
    }

    private TimeSpan NearestArrival(Transport transport, int stopNumber, TimeOnly currentTime)
    {
        int[] prefixSums = new int[transport.TimeBetweenStops.Length + 1];

        for (int i = 1; i < prefixSums.Length; i++)
            prefixSums[i] = transport.TimeBetweenStops[i - 1] + prefixSums[i - 1];

        if (currentTime < transport.StartsWorkingAt)
        {
            return transport.StartsWorkingAt - currentTime + TimeSpan.FromMinutes(prefixSums[stopNumber - 1]);
        }

        int minutesDifference = (currentTime - transport.StartsWorkingAt).Minutes;
        double loopsPassed = (double) minutesDifference / prefixSums[^1];

        int lesserArrival = (int) loopsPassed * prefixSums[^1] + prefixSums[stopNumber - 1] - minutesDifference;

        int greaterArrival = (int) Math.Ceiling(loopsPassed) * prefixSums[^1] + prefixSums[stopNumber - 1] -
                             minutesDifference;

        return TimeSpan.FromMinutes(minutesDifference <= lesserArrival ? lesserArrival : greaterArrival);
    }
}