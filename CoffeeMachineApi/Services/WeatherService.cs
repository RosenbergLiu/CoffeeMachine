using System.Text.Json;
using CoffeeMachineApi.Models;

namespace CoffeeMachineApi.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    public WeatherService(HttpClient httpClient, IConfiguration Configuration)
    {
        _httpClient = httpClient;
        _apiKey = Configuration.GetSection("ApiKeys")["OpenWeather"];
    }
    
    public async Task<double?> GetCurrentTemperatureAsync(string ipAddress)
    {
        #if DEBUG
        ipAddress = "124.189.10.81";
        #endif
        
        string getGeoLocationUrl = $"http://ip-api.com/json/{ipAddress}";
        var geoLocationResRaw = await _httpClient.GetAsync(getGeoLocationUrl);
        var geoLocationResJson = await geoLocationResRaw.Content.ReadAsStringAsync();
        GeoLocationRes geoLocationRes = JsonSerializer.Deserialize<GeoLocationRes>(geoLocationResJson);
        if (geoLocationRes is null){ return null; }
        
        string openWeatherUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={geoLocationRes.Lat}&lon={geoLocationRes.Lon}&appid={_apiKey}";
        var openWeatherResRaw = await _httpClient.GetAsync(openWeatherUrl);
        var openWeatherResJson = await openWeatherResRaw.Content.ReadAsStringAsync();
        OpenWeatherRes openWeatherRes = JsonSerializer.Deserialize<OpenWeatherRes>(openWeatherResJson);

        return openWeatherRes.Main.Temp;
    }
}