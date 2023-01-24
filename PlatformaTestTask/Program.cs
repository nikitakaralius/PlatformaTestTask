using PlatformaTestTask.Model;
using PlatformaTestTask.RouteBuilders;

string fileName = "schedule.txt";

if (File.Exists(fileName) == false)
{
    Console.Write("Enter schedule file path = ");
    fileName = Path.GetFullPath(Console.ReadLine()!);
}

string[] lines = File.ReadAllLines(fileName);
int n = int.Parse(lines[0]);

TimeOnly[] arrivalTime = lines[2].Split().Select(TimeOnly.Parse).ToArray();
int[] costsData = lines[3].Split().Select(int.Parse).ToArray();

var transport = new List<Transport>(n);

for (int i = 0; i < n; i++)
{
    int[] data = lines[4 + i].Split().Select(int.Parse).ToArray();
    int stopCount = data[0];

    var route = new Transport
    {
        BusNumber = i + 1,
        StopNumbers = data[1..(stopCount + 1)],
        TimeBetweenStops = data[(stopCount + 1)..],
        StartsWorkingAt = arrivalTime[i],
        Cost = costsData[i]
    };

    transport.Add(route);
}

Console.Write("Initial Stop = ");
int initial = int.Parse(Console.ReadLine()!);

Console.Write("Final Stop = ");
int finalStop = int.Parse(Console.ReadLine()!);

Console.Write("Start Time = ");
var startTime = TimeOnly.Parse(Console.ReadLine()!);

var departure = new Departure
{
    InitialStop = initial,
    FinalStop = finalStop,
    StartTime = startTime
};

Console.WriteLine();

Console.WriteLine("The Fastest Path:");
var fastestPath = new FastestRouteBuilder(transport).Build(departure);
Console.WriteLine(fastestPath.Verbose());

Console.WriteLine("The Cheapest Path:");
var cheapestPath = new CheapestRouteBuilder(transport).Build(departure);
Console.WriteLine(cheapestPath.Verbose());

