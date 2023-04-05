namespace CorePlugin.Plugin.Dtos;

public class FileDto
{
    [Required] public string FileName { get; set; } = null!;
    [Required] public string FullPath { get; set; } = null!;
}
