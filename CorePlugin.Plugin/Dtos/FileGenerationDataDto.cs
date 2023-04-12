namespace CorePlugin.Plugin.Dtos;

public class FileGenerationDataDto
{
    public class StudentPc
    {
        public string StudentName { get; set; } = null!;
        public int PcNr { get; set; }
    }
    //[Required] public string FileName { get; set; } = null!;
    [Required] public string ClazzName { get; set; } = null!;
    [Required] public string RoomName { get; set; } = null!;
    [Required] public string ImageBase64 { get; set; } = null!;
    [Required] public List<StudentPc> StudentPcList { get; set; } = new();

    public override string ToString() => $"{ClazzName}/{RoomName} with {StudentPcList.Count} students and image-size {ImageBase64.Length}";
}
