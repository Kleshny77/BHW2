using Microsoft.AspNetCore.Http;
var builder = WebApplication.CreateBuilder(args);

// Явно указываем порт 80 для прослушивания внутри контейнера
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80); // Слушаем на порту 80 на всех интерфейсах
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddHttpClient<ApiGateway.Services.FileStorageClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["FileStorageServiceUrl"]);
});
builder.Services.AddHttpClient<ApiGateway.Services.FileAnalysisClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["FileAnalysisServiceUrl"]);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "binary" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
// app.UseHttpsRedirection();

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
