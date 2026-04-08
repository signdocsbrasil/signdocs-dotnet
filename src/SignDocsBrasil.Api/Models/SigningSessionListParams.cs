using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class SigningSessionListParams
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("nextToken")]
    public string? NextToken { get; set; }

    [JsonPropertyName("startDate")]
    public string? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public string? EndDate { get; set; }

    public Dictionary<string, string> ToQueryDictionary()
    {
        var dict = new Dictionary<string, string>();

        if (Status is not null)
            dict["status"] = Status;
        if (Limit is not null)
            dict["limit"] = Limit.Value.ToString();
        if (NextToken is not null)
            dict["nextToken"] = NextToken;
        if (StartDate is not null)
            dict["startDate"] = StartDate;
        if (EndDate is not null)
            dict["endDate"] = EndDate;

        return dict;
    }

    public SigningSessionListParams Clone()
    {
        return (SigningSessionListParams)MemberwiseClone();
    }
}
