using System.Text.Json;

namespace SignDocsBrasil.Api.Tests.Helpers;

internal static class FixtureLoader
{
    private static readonly string FixtureDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "fixtures"));

    internal static string Load(string filename) =>
        File.ReadAllText(Path.Combine(FixtureDir, filename));

    internal static JsonDocument LoadJson(string filename) =>
        JsonDocument.Parse(Load(filename));

    internal static string LoadResponseBody(string filename)
    {
        using JsonDocument doc = LoadJson(filename);
        JsonElement body = doc.RootElement.GetProperty("response").GetProperty("body");
        return body.GetRawText();
    }
}
