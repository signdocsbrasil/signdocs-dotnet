using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Identity of the requester creating a signing session or envelope,
/// distinct from the signer(s). When provided, SignDocs automatically:
/// <list type="number">
///   <item>
///     Emails each signer an invitation with their signing URL — when
///     <c>signer.email</c> differs from <c>owner.email</c> (case-insensitive).
///   </item>
///   <item>
///     Emails the owner a completion notification per signer completion
///     (and a final "all signed" message for envelopes).
///   </item>
/// </list>
/// Omit <c>Owner</c> to keep the traditional behavior: the caller delivers
/// signing URLs via their own channels and relies on webhooks for
/// completion state.
/// </summary>
public class Owner
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
