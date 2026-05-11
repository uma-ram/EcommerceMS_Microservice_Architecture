using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:80");

//Load ocelot.json config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();


var app = builder.Build();

// Health check endpoint — Docker Compose uses this
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

await app.UseOcelot();

app.Run();