using System.Text.Json;
using SignDocsBrasil.Api.Models;
using Xunit;

namespace SignDocsBrasil.Api.Tests;

/// <summary>Detached-signature fields on DownloadResponse (non-PDF transactions).</summary>
public class DownloadResponseTests
{
    [Fact]
    public void DeserializesDetachedSignatureFields()
    {
        // Non-PDF transactions come back as documentFormat "generic" with a
        // detached CAdES signature instead of an embedded signedUrl.
        const string json = """
        {
          "transactionId": "tx_2",
          "expiresIn": 900,
          "documentFormat": "generic",
          "originalUrl": "https://s3.example.com/document.docx",
          "signatureUrl": "https://s3.example.com/signature.p7s"
        }
        """;

        var resp = JsonSerializer.Deserialize<DownloadResponse>(json);

        Assert.NotNull(resp);
        Assert.Equal("generic", resp!.DocumentFormat);
        Assert.Equal("https://s3.example.com/signature.p7s", resp.SignatureUrl);
        Assert.Null(resp.SignedUrl);
    }

    [Fact]
    public void LeavesNewFieldsNullForAPdf()
    {
        const string json = """
        {"transactionId":"tx_1","expiresIn":900,"documentFormat":"pdf","signedUrl":"https://s3.example.com/signed.pdf"}
        """;

        var resp = JsonSerializer.Deserialize<DownloadResponse>(json);

        Assert.NotNull(resp);
        Assert.Null(resp!.SignatureUrl);
        Assert.Equal("pdf", resp.DocumentFormat);
    }
}
