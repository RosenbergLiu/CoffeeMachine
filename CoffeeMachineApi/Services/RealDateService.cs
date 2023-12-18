namespace CoffeeMachineApi.Services;

public class RealDateService : IDateService
{
    public DateTime GetCurrentDate()
    {
        return DateTime.Now;
    }
}