using CoffeeMachineApi.Models;
using CoffeeMachineApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace CoffeeMachineApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CoffeeMachineController : ControllerBase
{
    private readonly IDateService _dateService;

    public CoffeeMachineController(IDateService dateService)
    {
        _dateService = dateService;
    }

    [HttpGet(Name = "brew-coffee")]
    public IActionResult BrewCoffee()
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
            Prepared = currentDate.ToString("o")
        });
    }
}