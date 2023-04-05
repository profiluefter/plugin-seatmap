using Org.BouncyCastle.Tls;
using System.Reflection;

namespace CorePlugin.Plugin.Controllers;

[ApiController]
public class WordController : ControllerBase
{
    private readonly WordInteropService _wordInteropService;

    public WordController(WordInteropService wordInteropService) => _wordInteropService = wordInteropService;

    [HttpPost("Create")]
    public async Task<FileDto> CreateWordFile([FromBody] WinWordDto dto)
    {
        this.Log(dto.ToString());
        var fileDto = await _wordInteropService.CreateWordFile(dto);
        fileDto.FullPath = fileDto.FullPath.Replace("\\", "/");
        return fileDto;
    }

    //[HttpGet("Read")]
    //public async Task<FileContentResult> ReadFile(FileDto fileDto)
    //{
    //  this.Log($"{fileDto.FileName} from {fileDto.FullPath}");
    //  byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(fileDto.FullPath);

    //  return new FileContentResult(fileBytes, "application/octet-stream")
    //  {
    //    FileDownloadName = fileDto.FileName
    //  };
    //}

    [HttpGet("ReadFile")]
    public async Task<FileContentResult> ReadFile(string fileName, string fullPath)
    {
        this.Log($"{fileName} from {fullPath}");
        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        string mimeType = "application/octet-stream";// "application /vnd.openxmlformats";
        return File(fileBytes, mimeType, fileName);
        //var data = System.IO.File.ReadAllBytes(fileDto.FullPath);
        //var stream = new MemoryStream(data);
        //using var fileStream = new FileStream(fileDto.FullPath, FileMode.Create, FileAccess.Write);
        //await stream.CopyToAsync(fileStream);
    }

    [HttpGet("DownloadTestFile")]
    public async Task<FileContentResult> ReturnByteArray()
    {
        string folder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "generatedDocs");
        string filename = "quaxi.docx";
        string fullPath = Path.Combine(folder, filename);
        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

        return new FileContentResult(fileBytes, "application/octet-stream")
        {
            FileDownloadName = filename
        };
    }
}
