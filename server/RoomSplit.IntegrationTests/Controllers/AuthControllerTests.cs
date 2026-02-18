using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RoomSplit.IntegrationTests.Factories;
using RoomSplit.IntegrationTests.Helpers;

namespace RoomSplit.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<RoomSplitWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(RoomSplitWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var payload = new
        {
            fullName = "John Doe",
            email = $"john_{Guid.NewGuid():N}@example.com",
            password = "SecurePass123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        root.GetProperty("data").GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        root.GetProperty("data").GetProperty("user").GetProperty("fullName").GetString().Should().Be("John Doe");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid():N}@example.com";
        var payload = new
        {
            fullName = "User One",
            email,
            password = "SecurePass123"
        };

        // First registration
        var first = await _client.PostAsJsonAsync("/api/auth/register", payload);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - duplicate registration
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var email = $"login_{Guid.NewGuid():N}@example.com";
        var password = "SecurePass123";

        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Login User",
            email,
            password
        });

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var email = $"badlogin_{Guid.NewGuid():N}@example.com";

        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Bad Login User",
            email,
            password = "CorrectPassword123"
        });

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "WrongPassword999"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsUser()
    {
        // Arrange
        var token = await AuthHelper.RegisterAndGetToken(_client);
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetProperty("email").GetString().Should().NotBeNullOrEmpty();
        root.GetProperty("data").GetProperty("fullName").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetMe_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
