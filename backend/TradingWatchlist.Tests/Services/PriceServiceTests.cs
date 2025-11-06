using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using TradingWatchlist.Infrastructure.Services;
using Xunit;

namespace TradingWatchlist.Tests.Services;

public class PriceServiceTests
{
    [Fact]
    public async Task GetCurrentPriceAsync_ValidTicker_ReturnsPrice()
    {
        // Arrange
        var ticker = "AAPL";
        var expectedPrice = 226.50m;

        var mockResponse = new
        {
            chart = new
            {
                result = new[]
                {
                    new
                    {
                        meta = new
                        {
                            regularMarketPrice = expectedPrice
                        }
                    }
                }
            }
        };

        var httpClient = CreateMockHttpClient(
            JsonSerializer.Serialize(mockResponse),
            HttpStatusCode.OK
        );

        var service = new PriceService(httpClient);

        // Act
        var result = await service.GetCurrentPriceAsync(ticker);

        // Assert
        result.Should().NotBeNull();
        result!.Ticker.Should().Be(ticker);
        result.Price.Should().Be(expectedPrice);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetCurrentPriceAsync_InvalidTicker_ReturnsNull()
    {
        // Arrange
        var ticker = "INVALID123";
        
        var httpClient = CreateMockHttpClient(
            "Not Found",
            HttpStatusCode.NotFound
        );

        var service = new PriceService(httpClient);

        // Act
        var result = await service.GetCurrentPriceAsync(ticker);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentPriceAsync_NetworkError_ReturnsNull()
    {
        // Arrange
        var ticker = "AAPL";
        
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new PriceService(httpClient);

        // Act
        var result = await service.GetCurrentPriceAsync(ticker);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("AAPL")]
    [InlineData("MSFT")]
    [InlineData("TSLA")]
    [InlineData("GOOGL")]
    public async Task GetCurrentPriceAsync_MultipleValidTickers_ReturnsCorrectTicker(string ticker)
    {
        // Arrange
        var mockResponse = new
        {
            chart = new
            {
                result = new[]
                {
                    new
                    {
                        meta = new
                        {
                            regularMarketPrice = 100.00m
                        }
                    }
                }
            }
        };

        var httpClient = CreateMockHttpClient(
            JsonSerializer.Serialize(mockResponse),
            HttpStatusCode.OK
        );

        var service = new PriceService(httpClient);

        // Act
        var result = await service.GetCurrentPriceAsync(ticker);

        // Assert
        result.Should().NotBeNull();
        result!.Ticker.Should().Be(ticker);
    }

    [Fact]
    public async Task GetCurrentPriceAsync_MalformedJson_ReturnsNull()
    {
        // Arrange
        var ticker = "AAPL";
        
        var httpClient = CreateMockHttpClient(
            "{ invalid json",
            HttpStatusCode.OK
        );

        var service = new PriceService(httpClient);

        // Act
        var result = await service.GetCurrentPriceAsync(ticker);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMultiplePricesAsync_ValidTickers_ReturnsPrices()
    {
        // Arrange
        var tickers = new List<string> { "AAPL", "MSFT", "TSLA" };
        
        var mockResponse = new
        {
            chart = new
            {
                result = new[]
                {
                    new
                    {
                        meta = new
                        {
                            regularMarketPrice = 150.00m
                        }
                    }
                }
            }
        };

        var httpClient = CreateMockHttpClient(
            JsonSerializer.Serialize(mockResponse),
            HttpStatusCode.OK
        );

        var service = new PriceService(httpClient);

        // Act
        var result = await service.GetMultiplePricesAsync(tickers);

        // Assert
        result.Should().HaveCount(3);
        result.Should().ContainKeys(tickers);
        result.Values.Should().AllSatisfy(price => price.Should().BeGreaterThan(0));
    }

    [Fact]
    public async Task GetMultiplePricesAsync_EmptyList_ReturnsEmptyDictionary()
    {
        // Arrange
        var tickers = new List<string>();
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        var service = new PriceService(httpClient);

        // Act
        var result = await service.GetMultiplePricesAsync(tickers);

        // Assert
        result.Should().BeEmpty();
    }

    // Helper method to create mock HttpClient
    private HttpClient CreateMockHttpClient(string responseContent, HttpStatusCode statusCode)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            });

        return new HttpClient(mockHandler.Object);
    }
}

// Integration Test (optional - testet gegen echte Yahoo API)
public class PriceServiceIntegrationTests
{
    [Fact(Skip = "Integration test - only run manually")]
    public async Task GetCurrentPriceAsync_RealAPI_ValidTicker_ReturnsPrice()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new PriceService(httpClient);
        var ticker = "AAPL";

        // Act
        var result = await service.GetCurrentPriceAsync(ticker);

        // Assert - Bei echtem API-Call sollte ein Preis zurückkommen
        result.Should().NotBeNull();
        result!.Ticker.Should().Be(ticker);
        result.Price.Should().BeGreaterThan(0);
        
        // Log für manuelle Überprüfung
        Console.WriteLine($"Ticker: {result.Ticker}");
        Console.WriteLine($"Price: ${result.Price:F2}");
        Console.WriteLine($"Timestamp: {result.Timestamp}");
    }

    [Fact(Skip = "Integration test - only run manually")]
    public async Task GetCurrentPriceAsync_RealAPI_InvalidTicker_ReturnsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new PriceService(httpClient);
        var ticker = "INVALID_TICKER_XYZ123";

        // Act
        var result = await service.GetCurrentPriceAsync(ticker);

        // Assert
        result.Should().BeNull();
    }
}