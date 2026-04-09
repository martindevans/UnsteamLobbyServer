using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UnsteamLobbyServer.Protocol;

internal static class JsonHelper
{
    // Matches "key": "value" (quoted) or "key": value (unquoted, e.g. numbers and booleans)
    private static readonly Regex FieldPattern = new(@"""(\$?\w+)""\s*:\s*(?:""([^""]*)""|(\w+))", RegexOptions.Compiled);

    internal static IReadOnlyDictionary<string, string> ParseFields(Stream stream)
    {
        string json;
        using (var reader = new StreamReader(stream))
            json = reader.ReadToEnd();

        var result = new Dictionary<string, string>();
        foreach (Match m in FieldPattern.Matches(json))
        {
            var key = m.Groups[1].Value;
            var value = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Value;
            result[key] = value;
        }
        return result;
    }
}
