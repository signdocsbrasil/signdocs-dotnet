using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record SigningSessionBootstrap(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("purpose")] string? Purpose,
    [property: JsonPropertyName("signer")] SigningSessionBootstrap.BootstrapSignerData? Signer,
    [property: JsonPropertyName("steps")] List<SigningSessionBootstrap.BootstrapStepData>? Steps,
    [property: JsonPropertyName("locale")] string? Locale,
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt,
    [property: JsonPropertyName("document")] SigningSessionBootstrap.BootstrapDocumentData? Document = null,
    [property: JsonPropertyName("action")] SigningSessionBootstrap.BootstrapActionData? Action = null,
    [property: JsonPropertyName("appearance")] SigningSessionBootstrap.BootstrapAppearanceData? Appearance = null,
    [property: JsonPropertyName("returnUrl")] string? ReturnUrl = null,
    [property: JsonPropertyName("cancelUrl")] string? CancelUrl = null
)
{
    public record BootstrapSignerData(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("maskedEmail")] string? MaskedEmail = null,
        [property: JsonPropertyName("maskedCpf")] string? MaskedCpf = null,
        [property: JsonPropertyName("availableOtpChannels")] List<string>? AvailableOtpChannels = null,
        [property: JsonPropertyName("otpChannelSelectable")] bool? OtpChannelSelectable = null
    );

    public record BootstrapStepData(
        [property: JsonPropertyName("stepId")] string? StepId,
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("order")] int Order
    );

    public record BootstrapDocumentData(
        [property: JsonPropertyName("presignedUrl")] string? PresignedUrl = null,
        [property: JsonPropertyName("filename")] string? Filename = null,
        [property: JsonPropertyName("hash")] string? Hash = null
    );

    public record BootstrapActionData(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("reference")] string? Reference = null
    );

    public record BootstrapAppearanceData(
        [property: JsonPropertyName("brandColor")] string? BrandColor = null,
        [property: JsonPropertyName("logoUrl")] string? LogoUrl = null,
        [property: JsonPropertyName("companyName")] string? CompanyName = null,
        [property: JsonPropertyName("backgroundColor")] string? BackgroundColor = null,
        [property: JsonPropertyName("textColor")] string? TextColor = null,
        [property: JsonPropertyName("buttonTextColor")] string? ButtonTextColor = null,
        [property: JsonPropertyName("borderRadius")] string? BorderRadius = null,
        [property: JsonPropertyName("headerStyle")] string? HeaderStyle = null,
        [property: JsonPropertyName("fontFamily")] string? FontFamily = null
    );
}
