using System.Net;
using System.Text.Json;
using RoomSplit.IntegrationTests.Factories;
using RoomSplit.IntegrationTests.Helpers;

namespace RoomSplit.IntegrationTests.Controllers;

public class TransactionsControllerTests : IClassFixture<RoomSplitWebAppFactory>
{
    private readonly HttpClient _client;

    public TransactionsControllerTests(RoomSplitWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTransaction_ReturnsSuccess()
    {
        // Arrange
        await AuthHelper.AuthenticateClient(_client);

        var form = new MultipartFormDataContent
        {
            { new StringContent("1"), "type" },
            { new StringContent("50000"), "amount" },
            { new StringContent("Test Expense"), "description" },
            { new StringContent("0"), "expenseCategory" },
            { new StringContent("2025-01-15"), "date" }
        };

        // Act
        var response = await _client.PostAsync("/api/transactions", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetProperty("amount").GetDecimal().Should().Be(50000);
        root.GetProperty("data").GetProperty("description").GetString().Should().Be("Test Expense");
    }

    [Fact]
    public async Task GetTransactions_ReturnsList()
    {
        // Arrange
        await AuthHelper.AuthenticateClient(_client);

        // Create a transaction first
        var form = new MultipartFormDataContent
        {
            { new StringContent("1"), "type" },
            { new StringContent("25000"), "amount" },
            { new StringContent("Grocery Shopping"), "description" },
            { new StringContent("0"), "expenseCategory" },
            { new StringContent("2025-01-20"), "date" }
        };
        await _client.PostAsync("/api/transactions", form);

        // Act
        var response = await _client.GetAsync("/api/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
    }
}
