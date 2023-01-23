namespace PlatformaTestTask.Model;

internal sealed record RouteNodeId(int BusNumber, int StopNumber)
{
    public static readonly RouteNodeId Initial = new(-1, -1);
}

internal interface IRouteNode
{
    RouteNodeId Id { get; }

    TransitionCost TransitionCost { get; }
}

internal sealed class RouteNode : IRouteNode
{
    public RouteNodeId Id { get; init; } = default!;

    public TransitionCost TransitionCost { get; init; } = default!;
}

internal sealed class TransportChangeRouteNode : IRouteNode
{
    public RouteNodeId Id { get; init; } = default!;

    public RouteNodeId From { get; init; } = default!;

    public ITransitionDelegate Delegate { get; init; } = default!;

    public TransitionCost TransitionCost => Delegate.CalculateTransitionCost(From, Id);
}
