using Core.Plugin.Interface;
using GrueneisR.RestClientGenerator;

namespace CorePlugin.Plugin;

public class Plugin : ICorePlugin
{
    readonly string restClientFolder = Environment.CurrentDirectory;
    readonly string restClientFilename = "_requests.http";

    public void ConfigureServices(WebApplicationBuilder builder)
    {
        Console.WriteLine("Plugin.ConfigureServices");
        builder.Services.AddControllers();
        builder.Services.AddCors();

        builder.Services.AddRestClientGenerator(options => options
          .SetFolder(restClientFolder)
          .SetFilename(restClientFilename)
          .SetAction($"swagger/v1/swagger.json")
        );

        builder.Services.AddSingleton<WordInteropService>();
    }

    public void Configure(WebApplication app)
    {
        Console.WriteLine("Plugin.Configure");
        app.UseRestClientGenerator();
        app.MapControllers();
    }
}
