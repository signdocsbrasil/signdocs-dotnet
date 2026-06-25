using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class Signer
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("userExternalId")]
    public string? UserExternalId { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("cpf")]
    public string? Cpf { get; set; }

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("birthDate")]
    public string? BirthDate { get; set; }

    [JsonPropertyName("otpChannel")]
    public string? OtpChannel { get; set; }

    [JsonPropertyName("otpChannelSelectable")]
    public bool? OtpChannelSelectable { get; set; }
}
