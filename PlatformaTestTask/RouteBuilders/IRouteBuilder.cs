using PlatformaTestTask.Model;

namespace PlatformaTestTask.RouteBuilders;

internal interface IRouteBuilder
{
    Route Build(Departure departure);
}
