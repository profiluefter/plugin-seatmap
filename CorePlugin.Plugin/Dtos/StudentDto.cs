namespace CorePlugin.Plugin.Dtos;

public class StudentDto
{
    [Required] public string Lastname { get; set; } = null!;
    [Required] public string Firstname { get; set; } = null!;
    [Required] public string ClazzName { get; set; } = null!;
    [Required] public int PostalCode { get; set; }
    [Required] public string City { get; set; } = null!;
}
