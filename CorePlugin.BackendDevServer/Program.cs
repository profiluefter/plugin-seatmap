using Core.AuthLib;
using CorePlugin.Plugin;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x => x.AddSwaggerGenHeader());

// Add Authentication
builder.AddHeaderAuth();

/*
 *  ___   ___    _  _  ___ _____   _____ ___  _   _  ___ _  _
 * |   \ / _ \  | \| |/ _ \_   _| |_   _/ _ \| | | |/ __| || |
 * | |) | (_) | | .` | (_) || |     | || (_) | |_| | (__| __ |
 * |___/ \___/  |_|\_|\___/ |_|     |_| \___/ \___/ \___|_||_|
 *
 * No dependency injection or similar specific for the plugin should be configured here.
 */
var plugin = new Plugin();
plugin.ConfigureServices(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

plugin.Configure(app);

app.Run();
