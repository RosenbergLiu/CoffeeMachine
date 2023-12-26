using System.Net;
using CoffeeMachineApi.Models;

namespace CoffeeMachineApi.Services;

public interface IWeatherService
{
    Task<WeatherServiceRes> GetTemperatureAsync(IPAddress ipAddress);
}