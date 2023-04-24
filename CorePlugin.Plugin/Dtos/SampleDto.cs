using System.ComponentModel.DataAnnotations;

namespace CorePlugin.Plugin.Dtos;

#warning This is a sample DTO, it should be deleted in your plugin.
public class SampleDto
{
    [Required] public string Description { get; set; } = null!;
}
