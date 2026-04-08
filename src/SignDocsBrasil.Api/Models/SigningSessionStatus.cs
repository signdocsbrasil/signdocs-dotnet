using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record SigningSessionStatus(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("signers")] List<SigningSessionStatus.SignerStatus>? Signers,
    [property: JsonPropertyName("completedAt")] string? CompletedAt,
    [property: JsonPropertyName("updatedAt")] string? UpdatedAt
)
{
    public record SignerStatus(
        [property: JsonPropertyName("signerId")] string? SignerId,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("signedAt")] string? SignedAt,
        [property: JsonPropertyName("viewedAt")] string? ViewedAt
    );
}
