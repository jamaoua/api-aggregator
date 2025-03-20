using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using ApiAggregator.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using System.Net;

public class AggregatedDataControllerTests
{
    private readonly Mock<ILogger<AggregatedDataController>> _mockLogger;

    public AggregatedDataControllerTests()
    {
        _mockLogger = new Mock<ILogger<AggregatedDataController>>();
    }

    [Fact]
    public async Task SimpleFetch_ReturnsOkResult()
    {
        // Arrange
        var mockHttp = new Mock<HttpMessageHandler>();

        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"Temperature\": 25, \"Windspeed\": 5}")
            });

        var httpClient = new HttpClient(mockHttp.Object);
        var controller = new AggregatedDataController(httpClient, _mockLogger.Object);

        // Act
        var result = await controller.SimpleFetch("https://api.open-meteo.com/v1/forecast?latitude=37.98&longitude=23.72&current_weather=true");

        // Assert
        Assert.IsType<string>(result);
        Assert.Contains("\"Temperature\": 25", result);
    }
    [Fact]
    public async Task HeadersFetch_ReturnsOkResult()
    {
        // Arrange
        var mockHttp = new Mock<HttpMessageHandler>();

        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"Temperature\": 25, \"Windspeed\": 5}")
            });

        var httpClient = new HttpClient(mockHttp.Object);
        var controller = new AggregatedDataController(httpClient, _mockLogger.Object);

        // Act
        var result = await controller.HeadersFetch("https://api.open-meteo.com/v1/forecast?latitude=37.98&longitude=23.72&current_weather=true");

        // Assert
        Assert.IsType<string>(result);
        Assert.Contains("\"Temperature\": 25", result);
    }
}
