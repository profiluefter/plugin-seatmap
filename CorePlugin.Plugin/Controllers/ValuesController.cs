namespace CorePlugin.Plugin.Controllers;

[Route("[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    public record struct OkStatus(bool IsOk, int Nr, string? Error = null);

    //private readonly PersonsContext _db;
    //public ValuesController(PersonsContext db) => _db = db;

    [HttpGet("Persons")]
    public OkStatus GetPersons()
    {
        //this.Log();
        try
        {
            int nr = 666;// _db.Persons.Count();
            return new OkStatus(true, nr);
        }
        catch (Exception exc)
        {
            return new OkStatus(false, -1, exc.Message);
        }
    }
}
