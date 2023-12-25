using System.Text.Json.Serialization;

namespace CoffeeMachineApi.Models;

public class GeoLocationRes
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("city")]
    public string City { get; set; }
    
    [JsonPropertyName("lat")]
    public string Lat { get; set; }
    
    [JsonPropertyName("lon")]
    public string Lon { get; set; }
}