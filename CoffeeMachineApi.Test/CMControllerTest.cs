using CoffeeMachineApi.Controllers;
using CoffeeMachineApi.Models;
using CoffeeMachineApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoffeeMachineApi.Test;

[TestClass]
public class CMControllerTest
{
    private Mock<IDateService> _mockDateService;
    private CoffeeMachineController _controller;

    [TestInitialize]
    public void SetUp()
    {
        // Initialize mock object for IDateService
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
        var result = _controller.BrewCoffee();

        // Assert
        // Check if the result is of type ObjectResult
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
            
        var objectResult = result as ObjectResult;

        // Ensure the result is not null and the status code is 200
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(200, objectResult.StatusCode);

        // Check the content of the response
        var responseContent = objectResult.Value as CoffeeMachineRes;
        Assert.IsNotNull(responseContent);
        Assert.AreEqual("Your piping hot coffee is ready", responseContent.Message, "The respond message is not 'Your piping hot coffee is ready'");
        
        // Verify the date format
        string expectedDateFormat = "yyyy-MM-ddTHH:mm:ssK"; // ISO 8601 format
        string actualPreparedDate = responseContent.Prepared;
        DateTime parsedDate;
        

        // Try to parse the date string from the response to ensure it's in the expected format
        bool canParse = DateTime.TryParseExact(actualPreparedDate, expectedDateFormat, null, System.Globalization.DateTimeStyles.AssumeLocal, out parsedDate);
    
        Assert.IsTrue(canParse, "The prepared date is not in the expected format.");
    }
    
    [TestMethod]
    public void BrewCoffee_Returns503ServiceUnavailable_EveryFifthRequest()
    {
        for (int i = 1; i <= 10; i++) // Test for 10 requests to cover two cycles
        {
            // Act
            var result = _controller.BrewCoffee();

            // Assert
            if (i % 5 == 0)
            {
                // Every fifth request should be a 503 Service Unavailable
                Assert.IsInstanceOfType(result, typeof(ObjectResult));
                var objectResult = result as ObjectResult;
                Assert.IsNotNull(objectResult);
                Assert.AreEqual(503, objectResult.StatusCode);
                Assert.IsNull(objectResult.Value as CoffeeMachineRes);
            }
            else
            {
                // Other requests should be 200 OK
                Assert.IsInstanceOfType(result, typeof(ObjectResult));
                var objectResult = result as ObjectResult;
                Assert.IsNotNull(objectResult);
                Assert.AreEqual(200, objectResult.StatusCode);
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
        var result = _controller.BrewCoffee();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult, "The result should not be null on April Fool's Day.");
        Assert.AreEqual(418, objectResult.StatusCode);
    }
    
    [TestMethod]
    public async Task BrewCoffee_ThreadSafety_OfRequestCount()
    {
        // Arrange
        int numberOfSimultaneousRequests = 5;
        List<IActionResult> responses = new List<IActionResult>();

        // Act
        var tasks = new List<Task<IActionResult>>();
        for (int i = 0; i < numberOfSimultaneousRequests; i++)
        {
            tasks.Add(Task.Run(() => _controller.BrewCoffee()));
        }
        responses = (await Task.WhenAll(tasks)).ToList();

        // Get the private static field value for _requestCount using reflection
        var requestCount = typeof(CoffeeMachineController)?.GetField("_requestCount", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)?
            .GetValue(null);

        // Assert the correct request count
        Assert.AreEqual(numberOfSimultaneousRequests, requestCount, "The request count not match the number of made requests.");

        // Analysis of responses
        int okCount = 0;
        int serviceUnavailableCount = 0;
        foreach (var response in responses)
        {
            if (response is ObjectResult objectResult)
            {
                if (objectResult.StatusCode == 200)
                {
                    okCount++;
                }
                else if (objectResult.StatusCode == 503)
                {
                    serviceUnavailableCount++;
                }
            }
        }

        // 5 requests come in only 4 can return Ok
        Assert.AreEqual(4, okCount, "There should be 4 successful (200 OK) responses.");
        Assert.AreEqual(1, serviceUnavailableCount, "There should be 1 service unavailable (503) responses.");
    }
}