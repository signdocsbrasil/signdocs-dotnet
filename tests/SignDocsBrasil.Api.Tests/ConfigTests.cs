namespace SignDocsBrasil.Api.Tests;

public class ConfigTests
{
    [Fact]
    public void DefaultBaseUrl_IsProduction()
    {
        Assert.Equal("https://api.signdocs.com.br", SignDocsBrasilClientOptions.DefaultBaseUrl);
    }

    [Fact]
    public void DefaultTimeout_Is30Seconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(30), SignDocsBrasilClientOptions.DefaultTimeout);
    }

    [Fact]
    public void DefaultMaxRetries_Is5()
    {
        Assert.Equal(5, SignDocsBrasilClientOptions.DefaultMaxRetries);
    }

    [Fact]
    public void DefaultScopes_ContainExpectedValues()
    {
        string[] expected = { "transactions:read", "transactions:write", "steps:write", "evidence:read", "webhooks:write" };
        Assert.Equal(expected, SignDocsBrasilClientOptions.DefaultScopes);
    }

    [Fact]
    public void NewOptions_HasDefaults()
    {
        var options = new SignDocsBrasilClientOptions();

        Assert.Equal(SignDocsBrasilClientOptions.DefaultBaseUrl, options.BaseUrl);
        Assert.Equal(SignDocsBrasilClientOptions.DefaultTimeout, options.Timeout);
        Assert.Equal(SignDocsBrasilClientOptions.DefaultMaxRetries, options.MaxRetries);
        Assert.Equal(SignDocsBrasilClientOptions.DefaultScopes, options.Scopes);
        Assert.Null(options.ClientId);
        Assert.Null(options.ClientSecret);
        Assert.Null(options.PrivateKey);
        Assert.Null(options.Kid);
        Assert.Null(options.HttpClient);
        Assert.Null(options.Logger);
    }

    [Fact]
    public void Validate_ThrowsForMissingClientId()
    {
        var options = new SignDocsBrasilClientOptions { ClientSecret = "secret" };

        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("ClientId", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsForNullClientId()
    {
        var options = new SignDocsBrasilClientOptions
        {
            ClientId = null,
            ClientSecret = "secret"
        };

        Assert.Throws<ArgumentException>(() => options.Validate());
    }

    [Fact]
    public void Validate_ThrowsForEmptyClientId()
    {
        var options = new SignDocsBrasilClientOptions
        {
            ClientId = "",
            ClientSecret = "secret"
        };

        Assert.Throws<ArgumentException>(() => options.Validate());
    }

    [Fact]
    public void Validate_ThrowsForMissingBothSecretAndPrivateKey()
    {
        var options = new SignDocsBrasilClientOptions { ClientId = "test-id" };

        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("ClientSecret", ex.Message);
        Assert.Contains("PrivateKey", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsForPrivateKeyWithoutKid()
    {
        var options = new SignDocsBrasilClientOptions
        {
            ClientId = "test-id",
            PrivateKey = "-----BEGIN PRIVATE KEY-----\nfake\n-----END PRIVATE KEY-----"
        };

        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("Kid", ex.Message);
    }

    [Fact]
    public void Validate_PassesWithClientSecret()
    {
        var options = new SignDocsBrasilClientOptions
        {
            ClientId = "test-id",
            ClientSecret = "test-secret"
        };

        var exception = Record.Exception(() => options.Validate());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_PassesWithPrivateKeyAndKid()
    {
        var options = new SignDocsBrasilClientOptions
        {
            ClientId = "test-id",
            PrivateKey = "-----BEGIN PRIVATE KEY-----\nfake\n-----END PRIVATE KEY-----",
            Kid = "key-001"
        };

        var exception = Record.Exception(() => options.Validate());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_PassesWithBothSecretAndPrivateKey()
    {
        var options = new SignDocsBrasilClientOptions
        {
            ClientId = "test-id",
            ClientSecret = "test-secret",
            PrivateKey = "-----BEGIN PRIVATE KEY-----\nfake\n-----END PRIVATE KEY-----",
            Kid = "key-001"
        };

        var exception = Record.Exception(() => options.Validate());
        Assert.Null(exception);
    }

    [Fact]
    public void UsesClientSecret_ReturnsTrue_WhenClientSecretSet()
    {
        var options = new SignDocsBrasilClientOptions { ClientSecret = "secret" };
        Assert.True(options.UsesClientSecret);
    }

    [Fact]
    public void UsesClientSecret_ReturnsFalse_WhenClientSecretNull()
    {
        var options = new SignDocsBrasilClientOptions();
        Assert.False(options.UsesClientSecret);
    }

    [Fact]
    public void UsesClientSecret_ReturnsFalse_WhenClientSecretEmpty()
    {
        var options = new SignDocsBrasilClientOptions { ClientSecret = "" };
        Assert.False(options.UsesClientSecret);
    }

    [Fact]
    public void UsesPrivateKeyJwt_ReturnsTrue_WhenPrivateKeySet()
    {
        var options = new SignDocsBrasilClientOptions { PrivateKey = "key-pem" };
        Assert.True(options.UsesPrivateKeyJwt);
    }

    [Fact]
    public void UsesPrivateKeyJwt_ReturnsFalse_WhenPrivateKeyNull()
    {
        var options = new SignDocsBrasilClientOptions();
        Assert.False(options.UsesPrivateKeyJwt);
    }

    [Fact]
    public void UsesPrivateKeyJwt_ReturnsFalse_WhenPrivateKeyEmpty()
    {
        var options = new SignDocsBrasilClientOptions { PrivateKey = "" };
        Assert.False(options.UsesPrivateKeyJwt);
    }

    [Fact]
    public void TokenUrl_IsConstructedFromBaseUrl()
    {
        var options = new SignDocsBrasilClientOptions { BaseUrl = "https://api.example.com" };
        Assert.Equal("https://api.example.com/oauth2/token", options.TokenUrl);
    }

    [Fact]
    public void TokenUrl_UsesDefaultBaseUrl()
    {
        var options = new SignDocsBrasilClientOptions();
        Assert.Equal("https://api.signdocs.com.br/oauth2/token", options.TokenUrl);
    }

    [Fact]
    public void CustomBaseUrl_IsPreserved()
    {
        var options = new SignDocsBrasilClientOptions { BaseUrl = "https://staging.signdocs.com.br" };
        Assert.Equal("https://staging.signdocs.com.br", options.BaseUrl);
    }

    [Fact]
    public void CustomTimeout_IsPreserved()
    {
        var options = new SignDocsBrasilClientOptions { Timeout = TimeSpan.FromSeconds(60) };
        Assert.Equal(TimeSpan.FromSeconds(60), options.Timeout);
    }

    [Fact]
    public void CustomMaxRetries_IsPreserved()
    {
        var options = new SignDocsBrasilClientOptions { MaxRetries = 10 };
        Assert.Equal(10, options.MaxRetries);
    }

    [Fact]
    public void CustomScopes_ArePreserved()
    {
        var options = new SignDocsBrasilClientOptions { Scopes = new[] { "custom:scope" } };
        Assert.Single(options.Scopes);
        Assert.Equal("custom:scope", options.Scopes[0]);
    }
}
