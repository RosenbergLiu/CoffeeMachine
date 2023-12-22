using System.Text.Json.Serialization;

namespace CoffeeMachineApi.Models;

public class CoffeeMachineRes
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("prepared")]
    public string Prepared { get; set; }
}