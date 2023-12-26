using System.Net;

namespace CoffeeMachineApi.Services;

public interface IWeatherService
{
    Task<double?> GetTemperatureAsync(IPAddress? ipAddress);
}