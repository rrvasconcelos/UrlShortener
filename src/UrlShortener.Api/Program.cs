using UrlShortener.Application.Abstractions.ShortCode;
using UrlShortener.Infrastructure.Services.CodeGeneration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Short code generator configuration & DI
// Salt deve ser fornecido por Secret Manager, variável de ambiente ou cofre (Hashids:Salt)
builder.Services.AddSingleton<IShortCodeGenerator>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var salt = config["Hashids:Salt"]; // não commitado em appsettings
    var minLen = config.GetValue<int?>("Hashids:MinHashLength") ?? 7;
    if (string.IsNullOrWhiteSpace(salt))
    {
        throw new InvalidOperationException("Hashids:Salt is not configured. Set it via user-secrets, environment variables, or a secrets vault.");
    }
    return new HashidsShortCodeGenerator(salt, minLen);
});

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
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
