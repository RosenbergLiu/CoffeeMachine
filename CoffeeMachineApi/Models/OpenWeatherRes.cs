using System.Text.Json.Serialization;

namespace CoffeeMachineApi.Models;

public class OpenWeatherRes
{
    [JsonPropertyName("main")]
    public OpenWeatherResMain Main { get; set; }

    [JsonPropertyName("cod")]
    public string Cod { get; set; }

    public OpenWeatherRes()
    {
        Main = new OpenWeatherResMain();
        Cod = string.Empty;
    }
}