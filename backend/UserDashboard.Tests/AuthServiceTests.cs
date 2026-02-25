using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserDashboard.API.Data;
using UserDashboard.API.DTOs;
using UserDashboard.API.Services;
using Xunit;

namespace UserDashboard.Tests;

public class AuthServiceTests
{
    private AppDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private IConfiguration CreateConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "test-super-secret-jwt-key-that-is-at-least-32-characters" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" }
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task Register_ShouldCreateUser_WhenValidData()
    {
        var context = CreateInMemoryContext("register_test_1");
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var dto = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "password123"
        };

        var user = await service.RegisterAsync(dto);

        Assert.NotNull(user);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("john@example.com", user.Email);
        Assert.NotEqual("password123", user.PasswordHash);
    }

    [Fact]
    public async Task Register_ShouldThrow_WhenEmailAlreadyExists()
    {
        var context = CreateInMemoryContext("register_test_2");
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var dto = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "password123"
        };

        await service.RegisterAsync(dto);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(dto));
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenValidCredentials()
    {
        var context = CreateInMemoryContext("login_test_1");
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var registerDto = new RegisterDto
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Password = "password123"
        };
        await service.RegisterAsync(registerDto);

        var loginDto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "password123"
        };

        var token = await service.LoginAsync(loginDto);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenInvalidPassword()
    {
        var context = CreateInMemoryContext("login_test_2");
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var registerDto = new RegisterDto
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane2@example.com",
            Password = "password123"
        };
        await service.RegisterAsync(registerDto);

        var loginDto = new LoginDto
        {
            Email = "jane2@example.com",
            Password = "wrongpassword"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(loginDto));
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenUserNotFound()
    {
        var context = CreateInMemoryContext("login_test_3");
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var loginDto = new LoginDto
        {
            Email = "notfound@example.com",
            Password = "password123"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(loginDto));
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnUser_WhenExists()
    {
        var context = CreateInMemoryContext("getuser_test_1");
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var registerDto = new RegisterDto
        {
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            Password = "password123"
        };
        await service.RegisterAsync(registerDto);

        var user = await service.GetUserByEmailAsync("alice@example.com");

        Assert.NotNull(user);
        Assert.Equal("Alice", user.FirstName);
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnNull_WhenNotExists()
    {
        var context = CreateInMemoryContext("getuser_test_2");
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var user = await service.GetUserByEmailAsync("notfound@example.com");

        Assert.Null(user);
    }
}
