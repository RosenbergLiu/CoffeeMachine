using System.Text.Json.Serialization;

namespace CoffeeMachineApi.Models;

public class OpenWeatherResMain
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }

    public OpenWeatherResMain()
    {
        Temp = 0;
    }
}