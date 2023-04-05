namespace CorePlugin.Plugin.Models;
internal class Student
{
    public string Lastname { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string ClazzName { get; set; } = null!;
    public int PostalCode { get; set; }
    public string City { get; set; } = null!;
    public string Name => $"{Lastname}_{Firstname}";
    public string ImageName => $"{Name}.jpg";
    public string ImagePath => Path.Combine("img", ClazzName, ImageName);

    public override string ToString() => Name.Replace("_", " ");

    public static Student? Parse(string csvLine)
    {
        //       0               1               2         3      4
        //studentLongname;studentForename;studentKlasse;postCode;city
        //Bauer;Samuel;1m;4780;Schärding
        string[] items = csvLine.Split(";");
        try
        {
            return new Student
            {
                Lastname = items[0],
                Firstname = items[1],
                ClazzName = items[2],
                PostalCode = int.TryParse(items[3], out int code) ? code : 0,
                City = items[4],
            };
        }
        catch (Exception exc)
        {
            Console.WriteLine($"Error parsing line '{csvLine}' - Reason: {exc.Message}");
            return null;
        }
    }
}
