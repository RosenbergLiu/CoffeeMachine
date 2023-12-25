using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using CoffeeMachineApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CoffeeMachineApi.IntegrationTest;

[TestClass]
public class CMControllerIntegrationTest
{
    private static WebApplicationFactory<Program> _factory;
    private static HttpClient _client;
    private readonly Mock<IDateService> _mockDateService;
    
    public CMControllerIntegrationTest()
    {
        // Initialize mock object for IDateService
        _mockDateService = new Mock<IDateService>();
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
    }
    
    [TestMethod]
    public async Task BrewCoffee_ShouldReturnSuccessOnNormalRequest()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/brew-coffee");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        // Additional assertions here...
    }
}