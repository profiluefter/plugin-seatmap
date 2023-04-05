namespace CorePlugin.Plugin.Controllers;

[ApiController]
public class CsvController : ControllerBase
{
    [HttpGet("rooms")]
    public List<RoomDto> Rooms()
    {
        this.Log();
        return Config.Instance.RoomNames
            .Select(x => new RoomDto
            {
                Name = x,
                NrSeats = Config.Instance.SeatMap[x].Count
            })
            .ToList();
    }

    [HttpGet("seats")]
    public List<SeatDto> Seats(string roomName)
    {
        this.Log(roomName);
        return Config.Instance
          .SeatMap[roomName]
          .Select(x => new SeatDto().CopyPropertiesFrom(x))
          .OrderBy(x => x.Nr)
          .ToList();
    }

    [HttpGet("students")]
    public List<StudentDto> Students()
    {
        this.Log();
        return Config.Instance
          .Students
          .Select(x => new StudentDto().CopyPropertiesFrom(x))
          .OrderBy(x => x.Lastname)
          .ThenBy(x => x.Firstname)
          .ToList();
    }

    [HttpGet("classes")]
    public List<ClazzDto> Clazzes()
    {
        this.Log();
        return Config.Instance
          .ClazzNames
          .Select(x => new ClazzDto
          {
              Name = x,
              NrStudents = Config.Instance.Students.Where(y => y.ClazzName == x).Count(),
          })
          .OrderBy(x => x.Name)
          .ToList();
    }
}
