using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FleetOS.Tests;

public class SecurityHeadersIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SecurityHeadersIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsStrictTransportSecurity_WithMaxAge300()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Strict-Transport-Security");
        var hstsValue = response.Headers.GetValues("Strict-Transport-Security").First();
        hstsValue.Should().Contain("max-age=300");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsXFrameOptions_DENY()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Frame-Options");
        var value = response.Headers.GetValues("X-Frame-Options").First();
        value.Should().Be("DENY");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsXContentTypeOptions_Nosniff()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        var value = response.Headers.GetValues("X-Content-Type-Options").First();
        value.Should().Be("nosniff");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsReferrerPolicy_StrictOrigin()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Referrer-Policy");
        var value = response.Headers.GetValues("Referrer-Policy").First();
        value.Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsPermissionsPolicy_WithRequiredDirectives()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Permissions-Policy");
        var value = response.Headers.GetValues("Permissions-Policy").First();
        value.Should().Contain("camera");
        value.Should().Contain("microphone");
        value.Should().Contain("geolocation");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsXXssProtection_Zero()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-XSS-Protection");
        var value = response.Headers.GetValues("X-XSS-Protection").First();
        value.Should().Be("0");
    }

    [Fact]
    public async Task HealthEndpoint_DoesNotReturnXXssProtection_One()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert - X-XSS-Protection must NOT be "1" or "1; mode=block"
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (response.Headers.TryGetValues("X-XSS-Protection", out var values))
        {
            var value = values.First();
            value.Should().NotContain("1; mode=block", "deprecated XSS Auditor must be disabled");
            value.Should().NotBe("1", "XSS Auditor must be explicitly disabled with 0");
        }
    }
}
