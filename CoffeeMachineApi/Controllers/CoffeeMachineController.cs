using CoffeeMachineApi.Models;
using CoffeeMachineApi.Services;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> BrewCoffee()
    {
        DateTime currentDate = _dateService.GetCurrentDate();
        
        if (currentDate == new DateTime(DateTime.Now.Year, 4, 1))
        {
            return new StatusCodeResult(418);
        }

        return Ok(new CoffeeMachineRes()
        {
            Message = "Your piping hot coffee is ready",
            Prepared = currentDate.ToString("o")
        });
    }
}