using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using UserDashboard.API.Data;
using UserDashboard.API.DTOs;
using Xunit;

namespace UserDashboard.Tests;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    // Explicit shared root ensures all requests within this test share the same in-memory database.
    // (EF Core 8 uses scoped DbContextOptions, so without an explicit root each scope gets a fresh database.)
    private readonly InMemoryDatabaseRoot _dbRoot = new InMemoryDatabaseRoot();

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing PostgreSQL DbContextOptions registration
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database with a shared root so all request scopes see the same data
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTestDb", _dbRoot));
            });
        });
    }

    [Fact]
    public async Task Register_ShouldReturn200_WhenValidData()
    {
        var client = _factory.CreateClient();

        var dto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "password123"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldReturn409_WhenEmailAlreadyExists()
    {
        var client = _factory.CreateClient();

        var dto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "duplicate@example.com",
            Password = "password123"
        };

        await client.PostAsJsonAsync("/api/auth/register", dto);
        var response = await client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenValidCredentials()
    {
        var client = _factory.CreateClient();

        var registerDto = new RegisterDto
        {
            FirstName = "Login",
            LastName = "User",
            Email = "login@example.com",
            Password = "password123"
        };
        await client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new LoginDto
        {
            Email = "login@example.com",
            Password = "password123"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(content.TryGetProperty("token", out var token));
        Assert.NotEmpty(token.GetString()!);
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenInvalidCredentials()
    {
        var client = _factory.CreateClient();

        var loginDto = new LoginDto
        {
            Email = "notexist@example.com",
            Password = "wrongpassword"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUser_ShouldReturn401_WhenNoToken()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/user");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUser_ShouldReturnUserDetails_WhenAuthenticated()
    {
        var client = _factory.CreateClient();

        // Register
        var registerDto = new RegisterDto
        {
            FirstName = "Auth",
            LastName = "Test",
            Email = "authtest@example.com",
            Password = "password123"
        };
        await client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Login
        var loginDto = new LoginDto
        {
            Email = "authtest@example.com",
            Password = "password123"
        };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginContent.GetProperty("token").GetString()!;

        // Get user with token
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var userResponse = await client.GetAsync("/api/auth/user");
        var userContent = await userResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, userResponse.StatusCode);
        Assert.Equal("Auth", userContent.GetProperty("firstName").GetString());
        Assert.Equal("Test", userContent.GetProperty("lastName").GetString());
        Assert.Equal("authtest@example.com", userContent.GetProperty("email").GetString());
    }
}
