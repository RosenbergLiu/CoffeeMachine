using System.Globalization;
using System.Net;
using System.Text.Json;
using CoffeeMachineApi.Models;

namespace CoffeeMachineApi.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly string _apiKey;
    
    public WeatherService(IHttpClientFactory clientFactory, IConfiguration Configuration)
    {
        _clientFactory = clientFactory;
        _apiKey = Configuration.GetSection("ApiKeys")["OpenWeather"];
    }
    
    public async Task<double?> GetCurrentTemperatureAsync(IPAddress? ipAddress)
    {
        // Hard code the IP to a public IP address for testing purpose
        #if DEBUG
        ipAddress = IPAddress.Parse("124.189.10.81");
        #endif
        
        // Return null if the request comes from localhost
        if (ipAddress is null || ipAddress.Equals(IPAddress.Loopback) || ipAddress.Equals(IPAddress.IPv6Loopback))
        {
            // Unable to get the weather information
            return null;
        }
        
        var geoClient = _clientFactory.CreateClient();
        string getGeoLocationUrl = $"http://ip-api.com/json/{ipAddress}";
        var geoLocationResRaw = await geoClient.GetAsync(getGeoLocationUrl);
        var geoLocationResJson = await geoLocationResRaw.Content.ReadAsStringAsync();
        GeoLocationRes? geoLocationRes = JsonSerializer.Deserialize<GeoLocationRes>(geoLocationResJson);
        if (geoLocationRes is null){ return null; }

        string latitude = geoLocationRes.Lat.ToString(CultureInfo.InvariantCulture);
        string longitude = geoLocationRes.Lon.ToString(CultureInfo.InvariantCulture);
        var weatherClient = _clientFactory.CreateClient();
        string openWeatherUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={_apiKey}";
        var openWeatherResRaw = await weatherClient.GetAsync(openWeatherUrl);
        var openWeatherResJson = await openWeatherResRaw.Content.ReadAsStringAsync();
        OpenWeatherRes? openWeatherRes = JsonSerializer.Deserialize<OpenWeatherRes>(openWeatherResJson);
        if (openWeatherRes is null){ return null; }
        
        // Return a Celsius value
        return openWeatherRes.Main.Temp - 273.5;
    }
}