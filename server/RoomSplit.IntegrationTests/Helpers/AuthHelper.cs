using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RoomSplit.IntegrationTests.Helpers;

public static class AuthHelper
{
    private static int _counter = 0;

    public static async Task<string> RegisterAndGetToken(HttpClient client)
    {
        var count = Interlocked.Increment(ref _counter);
        var payload = new
        {
            fullName = $"Test User {count}",
            email = $"testuser{count}_{Guid.NewGuid():N}@example.com",
            password = "Test@12345"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", payload);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var token = doc.RootElement
            .GetProperty("data")
            .GetProperty("token")
            .GetString();

        return token!;
    }

    public static async Task AuthenticateClient(HttpClient client)
    {
        var token = await RegisterAndGetToken(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
