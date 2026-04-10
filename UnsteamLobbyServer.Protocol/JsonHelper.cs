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

internal struct JsonReader
{
    private readonly string _json;
    private int _index;

    public JsonReader(string json)
    {
        _json = json;
    }

    private bool ReadCharacter(char character)
    {
        return _json[_index] == character;
    }
    
    public bool ReadObjectStart()
    {
        return ReadCharacter('{');
    }

    public bool ReadEndObject()
    {
        return ReadCharacter('}');
    }

    public bool ReadPropertyName(string name)
    {
        if (!ReadCharacter('"'))
            return false;
        
        for (var i = 0; i < name.Length; i++)
            if (!ReadCharacter(name[i]))
                return false;

        if (!ReadCharacter('"'))
            return false;

        if (!ReadCharacter(':'))
            return false;

        return true;
    }
}
