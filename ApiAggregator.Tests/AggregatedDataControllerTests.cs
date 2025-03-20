using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using ApiAggregator.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class AggregatedDataControllerTests
{
    private readonly Mock<ILogger<AggregatedDataController>> _mockLogger;

    public AggregatedDataControllerTests()
    {
        _mockLogger = new Mock<ILogger<AggregatedDataController>>();
    }

    [Fact]
    public async Task GetAggregatedData_ReturnsOkResult()
    {
        // Arrange
        var mockHttp = new Mock<HttpClient>();
        var controller = new AggregatedDataController(mockHttp.Object, _mockLogger.Object);

        // Act
        var result = await controller.GetAggregatedData();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
