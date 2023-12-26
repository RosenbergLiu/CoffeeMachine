using System.Runtime.CompilerServices;
using System.Text.Json;
using CoffeeMachineApi.Services;

[assembly: InternalsVisibleTo("CoffeeMachineApi.IntegrationTest")]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
            
builder.Services.AddSingleton<IDateService, RealDateService>();

builder.Services.AddHttpClient();

builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();