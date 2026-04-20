using System.Net;

namespace SignDocsBrasil.Api.Tests;

public class ResponseMetadataTests
{
    [Fact]
    public void FromResponse_ParsesRateLimitIntegers()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("RateLimit-Limit", "1000");
        response.Headers.Add("RateLimit-Remaining", "750");
        response.Headers.Add("RateLimit-Reset", "60");

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "GET", "/v1/transactions");

        Assert.Equal(1000, metadata.RateLimitLimit);
        Assert.Equal(750, metadata.RateLimitRemaining);
        Assert.Equal(60, metadata.RateLimitReset);
    }

    [Fact]
    public void FromResponse_NonIntegerRateLimit_IsNull()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("RateLimit-Limit", "not-a-number");

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "GET", "/v1/x");

        Assert.Null(metadata.RateLimitLimit);
    }

    [Fact]
    public void FromResponse_MissingRateLimitHeaders_AreNull()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "GET", "/v1/x");

        Assert.Null(metadata.RateLimitLimit);
        Assert.Null(metadata.RateLimitRemaining);
        Assert.Null(metadata.RateLimitReset);
    }

    [Fact]
    public void FromResponse_ParsesDeprecation_UnixSecondsForm()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        // RFC 8594 @<unix-seconds> form
        response.Headers.Add("Deprecation", "@1735689600"); // 2025-01-01 00:00:00 UTC

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "GET", "/v1/x");

        Assert.NotNull(metadata.Deprecation);
        Assert.Equal(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), metadata.Deprecation);
        Assert.True(metadata.IsDeprecated());
    }

    [Fact]
    public void FromResponse_ParsesDeprecation_ImfFixdateForm()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        // IMF-fixdate (RFC 1123)
        response.Headers.Add("Deprecation", "Sun, 11 Nov 2029 08:49:37 GMT");

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "GET", "/v1/x");

        Assert.NotNull(metadata.Deprecation);
        Assert.Equal(2029, metadata.Deprecation!.Value.Year);
        Assert.Equal(11, metadata.Deprecation.Value.Month);
        Assert.Equal(11, metadata.Deprecation.Value.Day);
    }

    [Fact]
    public void FromResponse_ParsesSunset_UnixSecondsForm()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("Sunset", "@1735689600");

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "GET", "/v1/x");

        Assert.NotNull(metadata.Sunset);
        Assert.Equal(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), metadata.Sunset);
    }

    [Fact]
    public void FromResponse_UnparseableDeprecation_IsNull()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("Deprecation", "definitely not a date");

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "GET", "/v1/x");

        Assert.Null(metadata.Deprecation);
        Assert.False(metadata.IsDeprecated());
    }

    [Fact]
    public void FromResponse_RequestId_PrefersXRequestId()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("X-Request-Id", "req-123");
        response.Headers.Add("X-SignDocs-Request-Id", "sd-456");

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "GET", "/v1/x");

        Assert.Equal("req-123", metadata.RequestId);
    }

    [Fact]
    public void FromResponse_RequestId_FallsBackToXSignDocsRequestId()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("X-SignDocs-Request-Id", "sd-456");

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "GET", "/v1/x");

        Assert.Equal("sd-456", metadata.RequestId);
    }

    [Fact]
    public void FromResponse_NoRequestIdHeaders_IsNull()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "GET", "/v1/x");

        Assert.Null(metadata.RequestId);
    }

    [Fact]
    public void FromResponse_CapturesStatusMethodAndPath()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        ResponseMetadata metadata = ResponseMetadata.FromResponse(response, "post", "/v1/transactions");

        Assert.Equal(404, metadata.StatusCode);
        Assert.Equal("POST", metadata.Method);
        Assert.Equal("/v1/transactions", metadata.Path);
    }

    [Fact]
    public void IsDeprecated_FalseWhenDeprecationNull()
    {
        var metadata = new ResponseMetadata(
            null, null, null, null, null, null, 200, "GET", "/");

        Assert.False(metadata.IsDeprecated());
    }

    [Fact]
    public void IsDeprecated_TrueWhenDeprecationSet()
    {
        var metadata = new ResponseMetadata(
            null, null, null,
            DateTimeOffset.UtcNow, null, null, 200, "GET", "/");

        Assert.True(metadata.IsDeprecated());
    }
}
