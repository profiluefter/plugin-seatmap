using Microsoft.Office.Interop.Word;
using System.Reflection;

namespace CorePlugin.Plugin.Services;

public class WordInteropService
{
    private Application _word;
    private Document _doc;
    private object _filename = null!; //note: is required as type object, not string (see method Save2())
    private string? _folderPath = null;
    private string[] _currentStudentNames = Array.Empty<string>();
    private WinWordDto _winWordDto = null!;

    private const string FilenameImageMap = "__temporary_map.png";
    private const int FontSizeDefault = 14;
    private const int FontSizeHeader = 10;
    private Style? _styleNoSpacing;
    private const int TableNoBorders = 0; //0..no border, 1..border
    private const int TableWithBorders = 1; //0..no border, 1..border
    private const float CellPadding = 2;

    private static float CmToPoints(float cm) => cm * 28;
    private string BuildFilenameForFolder(string folderPath) => Path.Combine(folderPath, BuildFilenameWithoutFolder());
    private string BuildFilenameWithoutFolder() => $"{DateTime.Now:yyyy-MM-dd}_Sitzplan_{_winWordDto.RoomName}_{_winWordDto.ClazzName}.docx";
    private string GetDefaultFileLocation() => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "generatedDocs");

    private int GetPcNrOfStudent(string name) => _winWordDto.StudentPcList
      .FirstOrDefault(x => x.StudentName == name)
      ?.PcNr ?? 0;
    private string GetStudentOnPcNr(int nr) => _winWordDto.StudentPcList
      .FirstOrDefault(x => x.PcNr == nr)
      ?.StudentName ?? "-";

    public async Task<FileDto> CreateWordFile(WinWordDto winWordDto)
    {
        _winWordDto = winWordDto;
        _folderPath = GetDefaultFileLocation();
        _filename = BuildFilenameForFolder(_folderPath);
        Console.WriteLine($"Creating file {_filename} ...");
        _currentStudentNames = winWordDto.StudentPcList
          .Select(x => x.StudentName)
          .OrderBy(x => x)
          .ToArray();
        await System.Threading.Tasks.Task.Run(() => CreateWordDocument());
        Console.WriteLine($"File {_filename} saved!");
        return new FileDto
        {
            FileName = BuildFilenameWithoutFolder(),
            FullPath = _filename.ToString()!
        };
    }

    private void CreateWordDocument()
    {
        Console.WriteLine("CreateWordDocument");
        try
        {
            InitWordDocument();
            FillDocument();
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc.Message);
            TidyUpProcess();
        }
    }

    private void InitWordDocument()
    {
        Console.WriteLine("InitWordDocument");
        _word = new Application
        {
            DisplayAlerts = 0,
            Visible = false
        };
        _word.Options.CheckGrammarWithSpelling = false;
        _word.Options.CheckGrammarAsYouType = false;
        _word.Options.CheckSpellingAsYouType = false;
        _doc = _word.Documents.Add();
        _doc.PageSetup.BottomMargin = CmToPoints(1.5f);
        _doc.PageSetup.TopMargin = CmToPoints(1.5f);
        _doc.PageSetup.LeftMargin = CmToPoints(1.5f);
        _doc.PageSetup.RightMargin = CmToPoints(1.5f);

        foreach (Style currentStyle in _doc.Styles)
        {
            //Console.WriteLine($"   Style: {currentStyle.NameLocal}");
            if (currentStyle.NameLocal == "Kein Leerraum") _styleNoSpacing = currentStyle;
        }
        var style = _doc.Styles[WdBuiltinStyle.wdStyleHeading1];
        //style.Font.Name = "Segoe UI";
        style.Font.Size = 36;
        style.ParagraphFormat.PageBreakBefore = -1; //enable: -1, disable:0

        style = _doc.Styles[WdBuiltinStyle.wdStyleHeading2];
        style.Font.Size = 16;
        style.Font.Color = WdColor.wdColorBlack;
        style.ParagraphFormat.PageBreakBefore = 0;
        //Formatvorlage ändern - Rahmen -- Schattierung - Füllung
        style.Shading.BackgroundPatternColor = WdColor.wdColorGray20;

        //var firstParagraph = _doc.Paragraphs[1];
        //firstParagraph.LineSpacing = 10;
    }

    public void FillDocument()
    {
        Console.WriteLine("FillDocument");
        AddHeader($"{_winWordDto.RoomName} / {_winWordDto.ClazzName}\n");
        //AddFooter("");
        Paragraph paragraph;

        paragraph = AddHeading($"{_winWordDto.RoomName} / {_winWordDto.ClazzName} ({_currentStudentNames.Length} Schüler)");
        paragraph.Range.InsertParagraphAfter();
        paragraph = _doc.Content.Paragraphs.Add(paragraph.Range);
        AddSeatMap(paragraph);
        paragraph.Range.InsertParagraphAfter();

        _doc.Words.Last.InsertBreak(WdBreakType.wdPageBreak);
        paragraph = _doc.Content.Paragraphs.Add(paragraph.Range);
        AddSeatMapImage(paragraph);
        paragraph.Range.InsertParagraphAfter();

        _doc.SpellingChecked = true;
        _doc.GrammarChecked = true;

        Save(true);
    }

    public void Save(bool doDisposeAllReferences)
    {
        Console.WriteLine($"Saving {_filename}");
        _doc.SaveAs2(ref _filename);
        if (doDisposeAllReferences) TidyUpProcess();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    //static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    private void TidyUpProcess()
    {
        Console.WriteLine($"TidyUpProcess - Disposing all references");
        File.Delete(Path.Combine(_folderPath!, FilenameImageMap));
        int windowHandle = _word.ActiveWindow.Hwnd;
        GetWindowThreadProcessId((IntPtr)windowHandle, out uint idProcess);
        _doc.Close();
        _word.Quit();
        System.Runtime.InteropServices.Marshal.ReleaseComObject(_doc);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(_word);
        _doc = null;
        _word = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        //next lines: don't do it - it kills the WPF-App, not Word!
        //Process process = null;
        //try
        //{
        //  process = Process.GetProcessById((int)idProcess);
        //}
        //catch { }

        //if (process != null) process.Kill();
    }

    private void AddHeader(string text)
    {
        foreach (Section section in _doc.Sections)
        {
            var headerRange = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
            headerRange.Fields.Add(headerRange, WdFieldType.wdFieldPage);
            headerRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            headerRange.Font.ColorIndex = WdColorIndex.wdBlack;
            headerRange.Font.Size = FontSizeHeader;
            headerRange.Text = text;
        }
    }

    //private void AddFooter(string text)
    //{
    //  foreach (Section wordSection in doc.Sections)
    //  {
    //    Microsoft.Office.Interop.Word.Range footerRange = wordSection.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
    //    footerRange.Collapse(WdCollapseDirection.wdCollapseEnd);

    //    var totalPages = WdFieldType.wdFieldNumPages;
    //    var currentPage = WdFieldType.wdFieldPage;
    //    footerRange.Fields.Add(footerRange, totalPages);
    //    Paragraph paragraph = footerRange.Paragraphs.Add();
    //    paragraph.Range.Text = " von ";
    //    footerRange.Fields.Add(footerRange, currentPage);
    //    paragraph = footerRange.Paragraphs.Add();
    //    paragraph.Range.Text = "Seite ";
    //    footerRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
    //    //footerRange.Font.ColorIndex = WdColorIndex.wdDarkRed;
    //    footerRange.Font.Size = fontSizeFooter;
    //  }
    //}

    private Paragraph AddHeading(string title)
    {
        var paragraph = _doc.Content.Paragraphs.Add();
        var range = paragraph.Range;
        range.Text = title;

        //Note: The application of the style has to come after the text being added to the range
        //see: https://social.technet.microsoft.com/Forums/ie/en-US/caec93ef-a43c-43a7-8365-3e27df32c761/c-word-interoperability-headings-not-working?forum=word
        range.set_Style(WdBuiltinStyle.wdStyleHeading2);
        range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        //range.InsertParagraphAfter();
        return paragraph;
    }

    private void AddSeatMap(Paragraph paragraph)
    {
        Console.WriteLine($"AddSeatMap");
        string[] names = _currentStudentNames;
        int nrRows = names.Length;
        int widthPc = 30;
        int widthName = 180;
        int widthGap = 30;

        paragraph.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        //if (_styleNoSpacing != null) paragraph.set_Style(_styleNoSpacing);

        var table = _doc.Tables.Add(paragraph.Range, NumRows: nrRows, NumColumns: 5);
        table.Borders.Enable = TableNoBorders; //borders are set in cells
        table.Rows.Alignment = WdRowAlignment.wdAlignRowCenter;

        //Note: Table index starts with 1!
        Cell cell;
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            int row = i + 1;
            int pcNr = GetPcNrOfStudent(name);
            cell = CreateCell(widthName, table, row, 1);
            AddText(cell.Range, name, FontSizeDefault, isCentered: false, isBold: false);

            cell = CreateCell(widthPc, table, row, 2);
            AddText(cell.Range, $"{pcNr:00}", FontSizeDefault, isCentered: true, isBold: false);

            table.Cell(row, 3).Width = widthGap;
        }
        var pcNrs = _winWordDto.StudentPcList.Select(x => x.PcNr).OrderBy(x => x).ToList();
        for (int i = 0; i < pcNrs.Count; i++)
        {
            int pcNr = pcNrs[i];
            string name = GetStudentOnPcNr(pcNr);
            int row = i + 1;
            cell = CreateCell(widthPc, table, row, 4);
            AddText(cell.Range, $"{pcNr:00}", FontSizeDefault, isCentered: true, isBold: false);

            cell = CreateCell(widthName, table, row, 5);
            AddText(cell.Range, name, FontSizeDefault, isCentered: false, isBold: false);
        }
    }

    private static Cell CreateCell(int width, Table table, int row, int col)
    {
        var cell = table.Cell(row, col);
        cell.Width = width;
        cell.Borders.Enable = TableWithBorders;
        cell.BottomPadding = CellPadding;
        cell.TopPadding = CellPadding;
        cell.LeftPadding = CellPadding;
        cell.RightPadding = CellPadding;
        return cell;
    }

    private void AddSeatMapImage(Paragraph paragraph)
    {
        string imageFile = PrintMapAsImage();
        AddScaledPicture(paragraph.Range, imageFile, widthCm: 19);
    }
    private void AddScaledPicture(Microsoft.Office.Interop.Word.Range range, string imageFile, float widthCm)
    {
        try
        {
            InlineShape autoScaledInlineShape = range.InlineShapes.AddPicture(imageFile);
            float scaledWidth = autoScaledInlineShape.Width;
            float scaledHeight = autoScaledInlineShape.Height;
            float rel = scaledHeight / scaledWidth;
            autoScaledInlineShape.Delete();

            // Create a new Shape and fill it with the picture
            Shape shape = _doc.Shapes.AddShape(1, 0, 0, CmToPoints(widthCm), CmToPoints(widthCm) * rel);
            shape.Fill.UserPicture(imageFile);
            //shape.IncrementRotation(90); //not working

            // Convert the Shape to an InlineShape and disable Border
            InlineShape finalInlineShape = shape.ConvertToInlineShape();
            finalInlineShape.Range.Cut();// Cut the range of the InlineShape to clipboard
            range.Paste();// And paste it to the target Range
                          //foreach (Shape foundShape in doc.Content.ShapeRange)
                          //{
                          //  foundShape.IncrementRotation(90);
                          //}
        }
        catch (Exception exc)
        {
            AddText(range, $"*** Exception: {exc.Message}", FontSizeDefault, isCentered: false, isBold: true);
        }
    }
    private void AddText(Microsoft.Office.Interop.Word.Range rangeBase, string text, int fontSize, bool isCentered, bool isBold, bool isItalic = false)
    {
        var paragraph = _doc.Content.Paragraphs.Add(rangeBase);
        var range = paragraph.Range;
        range.Text = text;
        range.Font.Bold = isBold ? 1 : 0;
        range.Font.Italic = isItalic ? 1 : 0;
        range.Font.Name = "Calibri (Textkörper)";
        range.Font.Size = fontSize;
        //cell.Shading.BackgroundPatternColor = WdColor.wdColorGray25;
        //cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
        range.ParagraphFormat.Alignment = isCentered
          ? WdParagraphAlignment.wdAlignParagraphCenter
          : WdParagraphAlignment.wdAlignParagraphLeft;
    }

    public string PrintMapAsImage()
    {
        Console.WriteLine("PrintMapAsImage");
        string imageFilename = Path.Combine(_folderPath, FilenameImageMap);
        Console.WriteLine($"  imageFilename={imageFilename}");

        string payloadBase64 = _winWordDto.ImageBase64;
        payloadBase64 = payloadBase64.Split(",")[1]; //data:image/png;base64,iVBORw..
        byte[]? payload = null;
        //seems that trailing "=" are missing --> try to add per code
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"    attempt {i}...");
            try
            {
                payload = Convert.FromBase64String(payloadBase64);
                break;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                payloadBase64 += "=";
            }
        }
        if (payload != null)
        {
            File.WriteAllBytes(imageFilename, payload!);
            Console.WriteLine($"Diagram written to {imageFilename}");
        }
        else
        {
            Console.WriteLine("Cannot add diagram");
            Console.WriteLine(_winWordDto.ImageBase64);
        }
        return imageFilename;
    }
}
