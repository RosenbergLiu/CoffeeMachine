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

    public CoffeeMachineController(IDateService dateService, IWeatherService weatherService)
    {
        _dateService = dateService;
        _weatherService = weatherService;
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
        
        //Get client IP
        string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        double? temp = await _weatherService.GetCurrentTemperatureAsync(ip);

        if (temp is not null & temp > 30)
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