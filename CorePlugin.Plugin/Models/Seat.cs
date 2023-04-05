namespace CorePlugin.Plugin.Models;
internal class Seat
{
    //nr;row;col;tofront
    public int Nr { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsToFront { get; set; }
    public bool IsBlocked { get; set; } = false;
    public override string ToString() => $"#{Nr} ({Row}/{Col})";

    public static Seat? Parse(string csvLine)
    {
        //nr;row;col;tofront;isblocked
        //1;1;1;0;1
        string[] items = csvLine.Split(";");
        try
        {
            var seat = new Seat
            {
                Nr = int.Parse(items[0]),
                Row = int.Parse(items[1]),
                Col = int.Parse(items[2]),
                IsToFront = int.Parse(items[3]) == 1,
            };
            if (items.Length > 4) seat.IsBlocked = (int.TryParse(items[4], out int val) ? val : 0) == 1;
            return seat;
        }
        catch (Exception exc)
        {
            Console.WriteLine($"Error parsing line '{csvLine}' - Reason: {exc.Message}");
            return null;

        }
    }
}
