using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class CreateSigningSessionRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("signers")]
    public List<SessionSigner>? Signers { get; set; }

    [JsonPropertyName("documents")]
    public List<SessionDocument>? Documents { get; set; }

    [JsonPropertyName("callbackUrl")]
    public string? CallbackUrl { get; set; }

    [JsonPropertyName("redirectUrl")]
    public string? RedirectUrl { get; set; }

    [JsonPropertyName("expiresInMinutes")]
    public int? ExpiresInMinutes { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("brandingId")]
    public string? BrandingId { get; set; }

    public class SessionSigner
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("cpf")]
        public string? Cpf { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("order")]
        public int? Order { get; set; }

        [JsonPropertyName("authMethods")]
        public List<string>? AuthMethods { get; set; }
    }

    public class SessionDocument
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }

        [JsonPropertyName("externalId")]
        public string? ExternalId { get; set; }
    }
}
