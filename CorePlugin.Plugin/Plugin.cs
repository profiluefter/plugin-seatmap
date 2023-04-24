using Core.Plugin.Interface;
using GrueneisR.RestClientGenerator;
using Core.Backend;
using Core.Plugin.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

        /*builder.Services.AddDbContext<PluginContext>(db =>
        {
            // USE THIS IF YOU WANT THAT THE PLUGIN WORKS IN PRODUCTION!!!!111
            var connectionString = builder.Configuration.GetConnectionStringThatAlsoWorksInProduction("PollsDatabaseConnection", builder.Environment.IsDevelopment());
            if (builder.Environment.IsDevelopment())
            {
                db.UseSqlite(connectionString);
            } else {
                db.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
        });*/
    }

    public void Configure(WebApplication app)
    {
        Console.WriteLine("Plugin.Configure");
        app.UseRestClientGenerator();
        app.MapControllers();
    }
}
