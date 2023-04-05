namespace CorePlugin.Plugin.Dtos;

public class SeatDto
{
    [Required] public int Nr { get; set; }
    [Required] public int Row { get; set; }
    [Required] public int Col { get; set; }
    [Required] public bool IsToFront { get; set; }
    [Required] public bool IsBlocked { get; set; }
}
