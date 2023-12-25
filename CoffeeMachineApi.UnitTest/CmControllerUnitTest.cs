using System.Diagnostics;
using CoffeeMachineApi.Controllers;
using CoffeeMachineApi.Models;
using CoffeeMachineApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoffeeMachineApi.UnitTest;

[TestClass]
public class CmControllerUnitTest
{
    private Mock<IDateService> _mockDateService;
    private CoffeeMachineController _controller;
    private const string ExpectedSuccessMessage = "Your piping hot coffee is ready";
    private const string ExpectedDateFormat = "yyyy-MM-ddTHH:mm:ssK"; // ISO 8601 format
    private const int NumberOfSimultaneousRequests = 5;
    
    [TestInitialize]
    public void Initialize()
    {
        _mockDateService = new Mock<IDateService>();
        // Setup mock to return a date that is not 1 of April
        _mockDateService.Setup(service => service.GetCurrentDate()).Returns(new DateTime(2023, 3, 2));

        // Initialize the controller with the mock object
        _controller = new CoffeeMachineController(_mockDateService.Object);
        
        // Reset the static request count before each test
        typeof(CoffeeMachineController)
            .GetField("_requestCount", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, 0);
    }
    
    [TestMethod]
    public void BrewCoffee_Returns200OK_ForNormalRequest()
    {
        // Act
        var result = _controller.BrewCoffee() as ObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);

        // Check the content of the response
        var responseContent = result.Value as CoffeeMachineRes;
        Assert.IsNotNull(responseContent, "The response content should not be null.");
        Assert.AreEqual(ExpectedSuccessMessage, responseContent.Message, "The respond message is not as expected.");

        // Verify the date format
        bool canParse = DateTime.TryParseExact(responseContent.Prepared, ExpectedDateFormat, null, System.Globalization.DateTimeStyles.AssumeLocal, out _);
        Assert.IsTrue(canParse, "The prepared date is not in the expected format.");
    }
    
    [TestMethod]
    public void BrewCoffee_Returns503ServiceUnavailable_EveryFifthRequest()
    {
        for (int i = 1; i <= 10; i++) // Test for 10 requests to cover two cycles
        {
            // Act
            var result = _controller.BrewCoffee() as ObjectResult;

            // Assert
            if (i % 5 == 0)
            {
                // Every fifth request should be a 503 Service Unavailable
                Assert.IsNotNull(result);
                Assert.AreEqual(503, result.StatusCode);
                Assert.IsNull(result.Value as CoffeeMachineRes);
            }
            else
            {
                // Other requests should be 200 OK
                Assert.IsNotNull(result);
                Assert.AreEqual(200, result.StatusCode);
            }
        }
    }
    
    [TestMethod]
    public void BrewCoffee_Returns418ImATeapot_OnAprilFoolsDay()
    {
        // Arrange
        var aprilFoolsDay = new DateTime(DateTime.Now.Year, 4, 1);
        _mockDateService.Setup(service => service.GetCurrentDate()).Returns(aprilFoolsDay);
        // Act
        var result = _controller.BrewCoffee() as ObjectResult;

        // Assert
        Assert.IsNotNull(result, "The result should not be null on April Fool's Day.");
        Assert.AreEqual(418, result.StatusCode);
    }
    
    [TestMethod]
    public async Task BrewCoffee_ThreadSafety_OfRequestCount()
    {
        // Act
        var responses = await Task.WhenAll(
            Enumerable.Range(0, NumberOfSimultaneousRequests).Select(_ => Task.Run(() => _controller.BrewCoffee()))
        );
        
        // Get the private static field value for _requestCount using reflection
        var requestCount = typeof(CoffeeMachineController)?.GetField("_requestCount", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)?
            .GetValue(null);
        // Assert the correct request count
        Assert.AreEqual(NumberOfSimultaneousRequests, requestCount, "The request count does not match the number of made requests.");

        // Analysis responses
        var objectResults = responses.Cast<ObjectResult>().ToList();
        int okCount = objectResults.Count(r => r.StatusCode == 200);
        int serviceUnavailableCount = objectResults.Count(r => r.StatusCode == 503);

        Assert.AreEqual(4, okCount, "There should be 4 successful (200 OK) responses.");
        Assert.AreEqual(1, serviceUnavailableCount, "There should be 1 service unavailable (503) response.");
    }
}