namespace CoffeeMachineApi.Services;

public interface IWeatherService
{
    Task<double?> GetCurrentTemperatureAsync(string ipAddress);
}