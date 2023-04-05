namespace CorePlugin.Plugin.Dtos;

public class ClazzDto
{
    [Required] public string Name { get; set; } = null!;
    [Required] public int NrStudents { get; set; }
}
