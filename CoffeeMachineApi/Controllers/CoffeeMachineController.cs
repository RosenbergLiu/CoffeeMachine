using System.Net;
using CoffeeMachineApi.Models;
using CoffeeMachineApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeMachineApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CoffeeMachineController : ControllerBase
{
    private static int _requestCount = 0;
    private readonly IDateService _dateService;
    private readonly IWeatherService _weatherService;
    private readonly ILogger<CoffeeMachineController> _logger;

    public CoffeeMachineController(IDateService dateService, IWeatherService weatherService, ILogger<CoffeeMachineController> logger)
    {
        _dateService = dateService;
        _weatherService = weatherService;
        _logger = logger;
    }

    [HttpGet("/brew-coffee")]
    public async Task<IActionResult> BrewCoffee()
    {
        int currentCount = Interlocked.Increment(ref _requestCount);
        
        // Return 503 on fifth request
        if (currentCount % 5 == 0)
        {
            return StatusCode(503, null);
        }
        
        // Return 418 on April 1
        DateTime currentDate = _dateService.GetCurrentDate();
        if (currentDate.Date == new DateTime(DateTime.Now.Year, 4, 1).Date)
        {
            return StatusCode(418, null);
        }
        
        // Get the client IP
        var ipAddress = HttpContext.Connection.RemoteIpAddress;
        
        // Get the celsius temperature based on client's IP location
        WeatherServiceRes? weatherServiceRes = await _weatherService.GetTemperatureAsync(ipAddress);
        
        // If the api have something wrong that cannot return temperature (null), return normal message.
        if (weatherServiceRes.Temperature is null)
        {
            _logger.LogWarning("Weather Service Unavailable");
        }
        else if (weatherServiceRes.Temperature > 30)
        {
            return StatusCode(200, new CoffeeMachineRes()
            {
                Message = "Your refreshing iced coffee is ready",
                Prepared = currentDate.ToString("yyyy-MM-ddTHH:mm:ssK")
            });
        }
        
        return StatusCode(200, new CoffeeMachineRes()
        {
            Message = "Your piping hot coffee is ready",
            Prepared = currentDate.ToString("yyyy-MM-ddTHH:mm:ssK")
        });
    }
}