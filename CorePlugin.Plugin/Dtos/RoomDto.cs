namespace CorePlugin.Plugin.Dtos;

public class RoomDto
{
    [Required] public string Name { get; set; } = null!;
    [Required] public int NrSeats { get; set; }
    [Required] public bool IsBig => NrSeats > 30;
}
