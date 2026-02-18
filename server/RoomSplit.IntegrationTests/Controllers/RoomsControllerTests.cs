using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RoomSplit.IntegrationTests.Factories;
using RoomSplit.IntegrationTests.Helpers;

namespace RoomSplit.IntegrationTests.Controllers;

public class RoomsControllerTests : IClassFixture<RoomSplitWebAppFactory>
{
    private readonly HttpClient _client;

    public RoomsControllerTests(RoomSplitWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateRoom_ReturnsCreatedRoom()
    {
        // Arrange
        await AuthHelper.AuthenticateClient(_client);

        var payload = new { name = "Test Room", description = "A test room" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/rooms", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetProperty("name").GetString().Should().Be("Test Room");
    }

    [Fact]
    public async Task GetRooms_ReturnsList()
    {
        // Arrange
        await AuthHelper.AuthenticateClient(_client);

        // Create a room first
        await _client.PostAsJsonAsync("/api/rooms", new { name = "Room for List Test" });

        // Act
        var response = await _client.GetAsync("/api/rooms");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }
}
