using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FleetOS.Tests;

public class SecurityHeadersIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SecurityHeadersIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsStrictTransportSecurity_WithMaxAge1Year()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Strict-Transport-Security");
        var hstsValue = response.Headers.GetValues("Strict-Transport-Security").First();
        hstsValue.Should().Contain("max-age=31536000");
        hstsValue.Should().Contain("includeSubDomains");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsXFrameOptions_DENY()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Frame-Options");
        var value = response.Headers.GetValues("X-Frame-Options").First();
        value.Should().Be("DENY");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsXContentTypeOptions_Nosniff()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        var value = response.Headers.GetValues("X-Content-Type-Options").First();
        value.Should().Be("nosniff");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsReferrerPolicy_StrictOrigin()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Referrer-Policy");
        var value = response.Headers.GetValues("Referrer-Policy").First();
        value.Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsPermissionsPolicy_WithRequiredDirectives()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Permissions-Policy");
        var value = response.Headers.GetValues("Permissions-Policy").First();
        value.Should().Contain("camera");
        value.Should().Contain("microphone");
        value.Should().Contain("geolocation");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsXXssProtection_Block()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-XSS-Protection");
        var value = response.Headers.GetValues("X-XSS-Protection").First();
        value.Should().Be("1; mode=block");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsContentSecurityPolicy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        csp.Should().Contain("default-src 'self'");
        csp.Should().Contain("script-src 'self'");
        csp.Should().Contain("frame-src 'none'");
        csp.Should().Contain("object-src 'none'");
    }
}
