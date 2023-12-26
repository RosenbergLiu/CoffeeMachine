using System.Text.Json.Serialization;

namespace CoffeeMachineApi.Models;

public class GeoLocationRes
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("city")]
    public string City { get; set; }
    
    [JsonPropertyName("lat")]
    public double Lat { get; set; }
    
    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    public GeoLocationRes()
    {
        Status = City = String.Empty;
        Lat = Lon = 0;
    }
}