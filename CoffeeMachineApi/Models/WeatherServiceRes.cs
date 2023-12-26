namespace CoffeeMachineApi.Models;

public class WeatherServiceRes
{
    public double? Temperature { get; set; }
    public string? Message { get; set; }

    public WeatherServiceRes(string errorMessage)
    {
        Temperature = null;
        Message = errorMessage;
    }

    public WeatherServiceRes(double temperature)
    {
        Temperature = temperature;
        Message = null;
    }
}