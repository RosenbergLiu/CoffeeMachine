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
            return StatusCode(503);
        }
        else
        {
#if DEBUG
            DateTime currentDate = _dateService.GetCurrentDate();
#else
            DateTime currentDate = DateTime.Today;
#endif

            if (currentDate == new DateTime(DateTime.Now.Year, 4, 1))
            {
                return StatusCode(418);
            }

            return StatusCode(200, new CoffeeMachineRes()
            {
                Message = "Your piping hot coffee is ready",
                Prepared = currentDate.ToString("yyyy-MM-ddTHH:mm:ssK")
            });
        }
    }
}