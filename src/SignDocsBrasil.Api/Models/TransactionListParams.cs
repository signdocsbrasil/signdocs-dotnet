using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class TransactionListParams
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("userExternalId")]
    public string? UserExternalId { get; set; }

    [JsonPropertyName("documentGroupId")]
    public string? DocumentGroupId { get; set; }

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
        if (UserExternalId is not null)
            dict["userExternalId"] = UserExternalId;
        if (DocumentGroupId is not null)
            dict["documentGroupId"] = DocumentGroupId;
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

    public TransactionListParams Clone()
    {
        return (TransactionListParams)MemberwiseClone();
    }
}
