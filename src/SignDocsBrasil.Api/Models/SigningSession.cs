using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record SigningSession(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("tenantId")] string? TenantId,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("signers")] List<SigningSession.SigningSessionSigner>? Signers,
    [property: JsonPropertyName("documents")] List<SigningSession.SigningSessionDocument>? Documents,
    [property: JsonPropertyName("callbackUrl")] string? CallbackUrl,
    [property: JsonPropertyName("redirectUrl")] string? RedirectUrl,
    [property: JsonPropertyName("sessionUrl")] string? SessionUrl,
    [property: JsonPropertyName("metadata")] Dictionary<string, string>? Metadata,
    [property: JsonPropertyName("locale")] string? Locale,
    [property: JsonPropertyName("brandingId")] string? BrandingId,
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt,
    [property: JsonPropertyName("createdAt")] string? CreatedAt,
    [property: JsonPropertyName("updatedAt")] string? UpdatedAt
)
{
    public record SigningSessionSigner(
        [property: JsonPropertyName("signerId")] string? SignerId,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("cpf")] string? Cpf,
        [property: JsonPropertyName("phone")] string? Phone,
        [property: JsonPropertyName("role")] string? Role,
        [property: JsonPropertyName("order")] int? Order,
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("signedAt")] string? SignedAt,
        [property: JsonPropertyName("signerUrl")] string? SignerUrl
    );

    public record SigningSessionDocument(
        [property: JsonPropertyName("documentId")] string? DocumentId,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("contentType")] string? ContentType,
        [property: JsonPropertyName("externalId")] string? ExternalId,
        [property: JsonPropertyName("status")] string? Status
    );
}
