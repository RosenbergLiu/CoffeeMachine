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

    public CoffeeMachineController(IDateService dateService)
    {
        _dateService = dateService;
    }

    [HttpGet("/brew-coffee")]
    public IActionResult BrewCoffee()
    {
        int currentCount = Interlocked.Increment(ref _requestCount);
        if (currentCount % 5 == 0)
        {
            HttpContext.Response.StatusCode = 503;
            return new EmptyResult();
        }
        else
        {
            DateTime currentDate = _dateService.GetCurrentDate();

            if (currentDate.Date == new DateTime(DateTime.Now.Year, 4, 1).Date)
            {
                HttpContext.Response.StatusCode = 418;
                return new EmptyResult();
            }

            return StatusCode(200, new CoffeeMachineRes()
            {
                Message = "Your piping hot coffee is ready",
                Prepared = currentDate.ToString("yyyy-MM-ddTHH:mm:ssK")
            });
        }
    }
}