var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        // Allocate a large amount of memory that will be immediately discarded
        var garbage = new byte[1024 * 1024 * 10]; // 10MB allocation
        
        // Create many short-lived objects to trigger frequent GC
        for (int i = 0; i < 10000; i++)
        {
            var temp = new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now), 
                Random.Shared.Next(-20, 55),
                new string('x', 1000) // Each creates a new 1KB string
            );
        }
        
        // Create many short-lived objects to trigger frequent GC
        for (int i = 0; i < 10000; i++)
        {
            var temp = new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now), 
                Random.Shared.Next(-20, 55),
                new string('x', 1000) // Each creates a new 1KB string
            );
        }
        
        var inefficentString = "";
        for (var i = 0; i < 1000; i++)
        {
            inefficentString += i.ToString();
        }
        
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}