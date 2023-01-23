namespace PlatformaTestTask.Model;

internal interface ITransitionDelegate
{
    TransitionCost CalculateTransitionCost(RouteNodeId from, RouteNodeId to);
}
