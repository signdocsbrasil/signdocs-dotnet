using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class Policy
{
    [JsonPropertyName("profile")]
    public string? Profile { get; set; }

    [JsonPropertyName("customSteps")]
    public List<string>? CustomSteps { get; set; }

    /// <summary>
    /// Minimum facial-match similarity this transaction requires, for the
    /// BIOMETRIC_MATCH and DOCUMENT_PHOTO_MATCH steps.
    /// </summary>
    /// <remarks>
    /// Tightens only: the value must be at or above the tenant's configured
    /// threshold, and anything lower is rejected with 400 naming the current
    /// minimum rather than being silently ignored — loosening identity
    /// checking is the tenant's decision, not the caller's. Accepts a
    /// percentage (95) or a fraction (0.95). Nullable so an unset bar is
    /// omitted rather than sent as 0, which the API would reject.
    /// </remarks>
    [JsonPropertyName("minSimilarity")]
    public double? MinSimilarity { get; set; }

    /// <summary>
    /// Minimum liveness confidence this transaction requires
    /// (BIOMETRIC_LIVENESS). Same rule as <see cref="MinSimilarity"/>.
    /// </summary>
    [JsonPropertyName("minLivenessConfidence")]
    public double? MinLivenessConfidence { get; set; }
}
