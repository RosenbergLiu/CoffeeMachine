using CoffeeMachineApi.Controllers;
using CoffeeMachineApi.Models;
using CoffeeMachineApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoffeeMachineApi.Test;

[TestClass]
public class CoffeeMachineControllerTest
{
    private Mock<IDateService> mockDateService;
    private CoffeeMachineController controller;

    [TestInitialize]
    public void SetUp()
    {
        // Initialize mock object for IDateService
        mockDateService = new Mock<IDateService>();

        // Setup mock to return the current date and time
        mockDateService.Setup(service => service.GetCurrentDate()).Returns(DateTime.Now);

        // Initialize the controller with the mock object
        controller = new CoffeeMachineController(mockDateService.Object);
        
        // Reset the static request count before each test
        typeof(CoffeeMachineController)
            .GetField("_requestCount", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, 0);
    }
    
    [TestMethod]
    public void BrewCoffee_Returns200OK_ForNormalRequest()
    {
        // Act
        var result = controller.BrewCoffee();

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
        Assert.AreEqual("Your piping hot coffee is ready", responseContent.Message);
        
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
            var result = controller.BrewCoffee();

            // Assert
            if (i % 5 == 0)
            {
                // Every fifth request should be a 503 Service Unavailable
                Assert.IsInstanceOfType(result, typeof(ObjectResult));
                var objectResult = result as ObjectResult;
                Assert.IsNotNull(objectResult);
                Assert.AreEqual(503, objectResult.StatusCode, $"Request {i} returns {objectResult.StatusCode}");
                Assert.IsNull(objectResult.Value as CoffeeMachineRes);
            }
            else
            {
                // Other requests should be 200 OK
                Assert.IsInstanceOfType(result, typeof(ObjectResult));
                var objectResult = result as ObjectResult;
                Assert.IsNotNull(objectResult);
                Assert.AreEqual(200, objectResult.StatusCode, $"Request {i} returns {objectResult.StatusCode}");
            }
        }
    }
}