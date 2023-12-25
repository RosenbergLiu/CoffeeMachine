using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using CoffeeMachineApi.Controllers;
using CoffeeMachineApi.Models;
using CoffeeMachineApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CoffeeMachineApi.IntegrationTest;

[TestClass]
public class CmControllerIntegrationTest
{
    private static WebApplicationFactory<Program> _factory;
    private static HttpClient _client;
    private readonly Mock<IDateService> _mockDateService;
    
    public CmControllerIntegrationTest()
    {
        _mockDateService = new Mock<IDateService>();
    }

    [TestInitialize]
    public void Initialize()
    {
        // Setup mock to return a date that is not 1 of April
        _mockDateService.Setup(service => service.GetCurrentDate()).Returns(new DateTime(2023, 3, 2));

        // Setup the factory for in-memory testing with the Program class
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace the real IDateService with a mock
                    services.AddScoped<IDateService>(_ => _mockDateService.Object);
                });
            });
        _client = _factory.CreateClient();
    
        // Reset the static request count before each test
        typeof(CoffeeMachineController)
            .GetField("_requestCount", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, 0);
    }
    
    [TestMethod]
    public async Task BrewCoffee_ShouldReturnSuccessOnNormalRequest()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/brew-coffee");

        // Assert - 200
        Assert.AreEqual((HttpStatusCode)200, response.StatusCode);

        // Assert - Message Content
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CoffeeMachineRes>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.AreEqual("Your piping hot coffee is ready", result?.Message, "The message content is not as expected.");
        
        // Assert - Date Format
        string dateFormat = "yyyy-MM-ddTHH:mm:ssK";  // The expected date format
        DateTime parsedDate;
        bool isValidDate = DateTime.TryParseExact(result.Prepared, dateFormat, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsedDate);
        Assert.IsTrue(isValidDate, $"The date '{result.Prepared}' is not in the expected format '{dateFormat}'.");
    }
    
    [TestMethod]
    public async Task BrewCoffee_ShouldReturnServiceUnavailableOnEveryFifthRequest()
    {
        // Arrange
        HttpResponseMessage? lastResponse = null;
            
        // Act - Send 5 requests
        for (int i = 1; i <= 5; i++)
        {
            var response = await _client.GetAsync("/brew-coffee");
            lastResponse = response;
        }

        // Assert - The 5th response should be ServiceUnavailable (503)
        Assert.AreEqual((HttpStatusCode)503, lastResponse?.StatusCode);
        // Assert - Empty response
        Assert.AreEqual("", await lastResponse?.Content.ReadAsStringAsync()!);
    }

    [TestMethod]
    public async Task BrewCoffee_ShouldReturnTeapotOnAprilFoolsDay()
    {
        // Arrange - Set the mock date to April 1st
        _mockDateService.Setup(service => service.GetCurrentDate()).Returns(new DateTime(2023, 4, 1));

        // Act
        var response = await _client.GetAsync("/brew-coffee");

        // Assert - 418
        Assert.AreEqual((HttpStatusCode)418, response.StatusCode);
        // Assert - Empty response
        Assert.AreEqual("", await response.Content.ReadAsStringAsync()!);
    }
    
    [TestMethod]
    public async Task BrewCoffee_ThreadSafety_OfRequestCount()
    {
        // Arrange
        int numberOfSimultaneousRequests = 5;
    
        // Act
        var tasks = Enumerable.Range(0, numberOfSimultaneousRequests)
            .Select(_ => _client.GetAsync("/brew-coffee"));

        List<HttpResponseMessage> responses = (await Task.WhenAll(tasks)).ToList();

        // Assert
        // Verify the total number of responses equals the number of requests sent
        Assert.AreEqual(numberOfSimultaneousRequests, responses.Count, "The number of responses should match the number of made requests.");

        // Analysis of responses
        int okCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        int serviceUnavailableCount = responses.Count(r => r.StatusCode == HttpStatusCode.ServiceUnavailable);

        // 5 requests come in; only 4 can return Ok if every fifth request is supposed to fail
        Assert.AreEqual(4, okCount, "There should be 4 successful (200 OK) responses.");
        Assert.AreEqual(1, serviceUnavailableCount, "There should be 1 service unavailable (503) response.");
    }
}