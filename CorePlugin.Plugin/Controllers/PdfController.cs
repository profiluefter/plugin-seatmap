using System.Reflection;

namespace CorePlugin.Plugin.Controllers;

[Route("[controller]")]
[ApiController]
public class PdfController : ControllerBase
{

    private static void GeneratePdfReport(string html, string fullPath, string title)
    {
        var renderer = new HtmlToPdf();
        CustomizePdf(renderer.RenderingOptions, title);
        var pdfDocument = renderer.RenderHtmlAsPdf(html);
        pdfDocument.SaveAs(fullPath);
    }

    private static void CustomizePdf(IronPdf.ChromePdfRenderOptions renderingOptions, string title)
    {
        Console.WriteLine($"CustomizePdf {title}");
        renderingOptions.SetCustomPaperSizeInInches(12.5, 20);
        renderingOptions.PrintHtmlBackgrounds = true;
        renderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Portrait;
        renderingOptions.Title = title;
        //renderingOptions.EnableJavaScript = true;
        renderingOptions.RenderDelay = 50; // in milliseconds
        renderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Screen;
        //renderingOptions.FitToPaperMode = FitToPaperModes.Zoom;
        renderingOptions.Zoom = 100;
        renderingOptions.CreatePdfFormsFromHtml = true;
        renderingOptions.MarginTop = 15; //millimeters
        renderingOptions.MarginLeft = 10; //millimeters
        renderingOptions.MarginRight = 10; //millimeters
        renderingOptions.MarginBottom = 15; //millimeters
    }

    [HttpGet("DownloadTestFile")]
    public async Task<FileContentResult> ReturnByteArray()
    {
        string html = """
       <!DOCTYPE html>
       <html lang="en">
       <head>
           This is the header of this document.
       </head>
      <body>
      <h1>This is the heading for demonstration purposes only.</h1>
      <p>This is a line of text for demonstration purposes only.</p>
      </body>
      </html>
      """;

        string folder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "generatedDocs");
        string filename = "quaxi.pdf";
        string fullPath = Path.Combine(folder, filename);
        GeneratePdfReport(html, fullPath, "Test page");
        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

        return new FileContentResult(fileBytes, "application/octet-stream")
        {
            FileDownloadName = filename
        };
    }

    [HttpPost("Create")]
    public FileDto CreatePdfFile([FromBody] FileGenerationDataDto dto)
    {
        string folder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "generatedDocs");
        string filename = $"{DateTime.Now:yyyy-MM-dd}_Sitzplan_{dto.RoomName}_{dto.ClazzName}.pdf";
        this.Log(filename);
        string fullPath = Path.Combine(folder, filename);
        string html = GenerateHtmlString(dto);
        WriteHtmlToFile(html, fullPath.Replace(".pdf", ".html"));
        GeneratePdfReport(html, fullPath, $"{DateTime.Now:yyyy-MM-dd} Sitzplan {dto.RoomName}/{dto.ClazzName}");
        var fileDto = new FileDto
        {
            FullPath = fullPath,
            FileName = filename
        };
        fileDto.FullPath = fileDto.FullPath.Replace("\\", "/");
        return fileDto;
    }
    private static void WriteHtmlToFile(string html, string fullPath)
    {
        Console.WriteLine($"WriteHtmlToFile {fullPath}");
        System.IO.File.WriteAllText(fullPath, html);
    }

    [HttpGet("ReadFile")]
    public async Task<FileContentResult> ReadFile(string fileName, string fullPath)
    {
        this.Log($"{fileName} from {fullPath}");
        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        string mimeType = "application/octet-stream";// "application /vnd.openxmlformats";
        return File(fileBytes, mimeType, fileName);
    }

    private string GenerateHtmlString(FileGenerationDataDto dto)
    {
        string head = GenerateHtmlHead();
        string styles = GenerateStyles();
        string body = GenerateHtmlBody(dto);
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine(head);
        sb.AppendLine(styles);
        sb.AppendLine("<body>");
        sb.AppendLine(body);
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    private static string GenerateStyles()
    {
        string folder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "templates");
        string styles = System.IO.File.ReadAllText(Path.Combine(folder, "styles.css"));
        return new StringBuilder()
          .AppendLine("<style>")
          .AppendLine(styles)
          .AppendLine("</style>")
          .ToString();
    }

    private static string GenerateHtmlHead() => $"<head>{DateTime.Now:dd.MM.yyyy}</head>";

    private string GenerateHtmlBody(FileGenerationDataDto dto)
    {
        var studentsByName = dto.StudentPcList.OrderBy(x => x.StudentName).ToList();
        var studentsByPcNr = dto.StudentPcList.OrderBy(x => x.PcNr).ToList();
        var sb = new StringBuilder();
        sb.AppendLine($"<h1 style=\"text-align: center;\">{dto.RoomName} / {dto.ClazzName} ({dto.StudentPcList.Count} Schüler)</h1>");
        sb.AppendLine("<table style=\"font-size: 20px; width:100%; border-collapse:collapse;\">");

        for (int i = 0; i < dto.StudentPcList.Count; i++)
        {
            AppendRow(sb, studentsByName[i], studentsByPcNr[i]);
        }

        sb.AppendLine("</table>");

        AppendImage(sb, dto);
        return sb.ToString();
    }

    private void AppendRow(StringBuilder sb, FileGenerationDataDto.StudentPc left, FileGenerationDataDto.StudentPc right)
    {
        sb.AppendLine("<tr>");
        sb.AppendLine($"  <td class=\"name-cell\">{left.StudentName}</td>");
        sb.AppendLine($"  <td class=\"nr-cell\">{left.PcNr:00}</td>");
        sb.AppendLine("  <td class=\"gap\">&nbsp;</td>");
        sb.AppendLine($"  <td class=\"nr-cell\">{right.PcNr:00}</td>");
        sb.AppendLine($"  <td class=\"name-cell\">{right.StudentName}</td>");
        sb.AppendLine("</tr>");
    }

    private void AppendImage(StringBuilder sb, FileGenerationDataDto dto)
    {
        sb.AppendLine("<div id=\"image-div\">");
        sb.AppendLine($"  <img src=\"{dto.ImageBase64}\" />");
        sb.AppendLine("</div>");
    }
}
