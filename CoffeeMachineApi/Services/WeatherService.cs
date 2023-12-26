using System.Globalization;
using System.Net;
using System.Text.Json;
using CoffeeMachineApi.Models;

namespace CoffeeMachineApi.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly string _apiKey;
    
    public WeatherService(IHttpClientFactory clientFactory, IConfiguration configuration)
    {
        _clientFactory = clientFactory;
        _apiKey = configuration.GetSection("ApiKeys")["OpenWeather"];
    }
    
    public async Task<WeatherServiceRes> GetTemperatureAsync(IPAddress ipAddress)
    {
        
#if DEBUG
        // Hard code the IP to a public IP address for localhost playground
        ipAddress = IPAddress.Parse("124.189.10.81");
#endif
        
        // Check if IP loopback
        if (ipAddress.Equals(IPAddress.Loopback) || ipAddress.Equals(IPAddress.IPv6Loopback))
        {
            return new WeatherServiceRes("IP is from localhost. Unable to get weather data for the IP location.");
        }
        
        try
        {
            var geoClient = _clientFactory.CreateClient();
            string getGeoLocationUrl = $"http://ip-api.com/json/{ipAddress}";
            var geoLocationResRaw = await geoClient.GetAsync(getGeoLocationUrl);
            var geoLocationResJson = await geoLocationResRaw.Content.ReadAsStringAsync();
            GeoLocationRes? geoLocationRes = JsonSerializer.Deserialize<GeoLocationRes>(geoLocationResJson);

            if (geoLocationRes is null || geoLocationRes.Status != "success")
            {
                return new WeatherServiceRes("IP lookup failed");
            }
            
            string latitude = geoLocationRes.Lat.ToString(CultureInfo.InvariantCulture);
            string longitude = geoLocationRes.Lon.ToString(CultureInfo.InvariantCulture);

            if (String.IsNullOrEmpty(latitude) || String.IsNullOrEmpty(longitude))
            {
                return new WeatherServiceRes("Fail to get the IP location");
            }
            
            var weatherClient = _clientFactory.CreateClient();
            string openWeatherUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={_apiKey}";
            var openWeatherResRaw = await weatherClient.GetAsync(openWeatherUrl);
            var openWeatherResJson = await openWeatherResRaw.Content.ReadAsStringAsync();
            OpenWeatherRes? openWeatherRes = JsonSerializer.Deserialize<OpenWeatherRes>(openWeatherResJson);
            if (openWeatherRes is null || openWeatherRes.Cod != 200)
            {
                return new WeatherServiceRes("Unable to get weather data"); 
            }
            
            // The OpenWeather API returns Kelvin value. We want the Celsius value
            return new WeatherServiceRes(openWeatherRes.Main.Temp - 273.5);
        }
        catch (Exception ex)
        {
            return new WeatherServiceRes(ex.Message);
        }
    }
}