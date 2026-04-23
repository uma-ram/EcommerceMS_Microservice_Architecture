
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:80");

builder.Services.AddControllers();


var app = builder.Build();


app.UseAuthorization();
app.MapControllers();

// Health check endpoint — Docker Compose uses this
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();