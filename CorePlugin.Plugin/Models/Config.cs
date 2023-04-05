namespace CorePlugin.Plugin.Models;
internal class Config
{
    private string BasePathCsv = "csv";
    private static Config? _instance = null;
    //public const string PathOutput = @"C:\Users\Besitzer\Desktop";
    public List<Student> Students { get; private set; } = new();
    public List<string> ClazzNames { get; private set; } = new();
    public Dictionary<string, List<Seat>> SeatMap { get; private set; } = new();
    public List<string> RoomNames { get; private set; } = new();

    private Config() { }

    public static Config Instance => _instance ??= new Config().Init();

    private Config Init()
    {
        Console.WriteLine("Config::Init");
        string location = System.Reflection.Assembly.GetEntryAssembly()!.Location;
        string exeDirectory = Path.GetDirectoryName(location)!;
        BasePathCsv = Path.Combine(exeDirectory, BasePathCsv);
        InitStudents();
        InitSeatMaps();
        return this;
    }

    private void InitStudents()
    {
        Console.WriteLine("Config::InitStudents");
        string path = Path.Combine(BasePathCsv, "students.csv");
        Students = File.ReadAllLines(path)
          .Skip(1)
          .Where(x => x.Trim().Any())
          .Select(x => Student.Parse(x))
          .Where(x => x != null)
          .Select(x => x!)
          .ToList();
        ClazzNames = Students.Select(x => x.ClazzName).Distinct().OrderBy(x => x).ToList();
    }

    private void InitSeatMaps()
    {
        Console.WriteLine("Config::InitSeatMaps");
        string prefix = "seatmappings_";
        var fileInfos = new DirectoryInfo(BasePathCsv).GetFiles($"{prefix}*.csv");
        foreach (var fileInfo in fileInfos)
        {
            string roomName = fileInfo.Name.Replace(fileInfo.Extension, "").Replace(prefix, "");
            var seats = File.ReadAllLines(fileInfo.FullName)
              .Skip(1)
              .Where(x => x.Trim().Any())
              .Select(x => Seat.Parse(x))
              .Where(x => x != null)
              .Select(x => x!)
              .ToList();
            SeatMap[roomName] = seats;
        }
        RoomNames = SeatMap.Keys.OrderBy(x => x).ToList();
    }

}
