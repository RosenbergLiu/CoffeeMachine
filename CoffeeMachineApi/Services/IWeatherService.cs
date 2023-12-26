using System.Net;

namespace CoffeeMachineApi.Services;

public interface IWeatherService
{
    Task<double?> GetCurrentTemperatureAsync(IPAddress? ipAddress);
}