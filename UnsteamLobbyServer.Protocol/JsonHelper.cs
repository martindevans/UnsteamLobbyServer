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

    private bool TryReadCharacter(char character)
    {
        if (_index >= _json.Length || _json[_index] != character)
            return false;
        _index++;
        return true;
    }

    public bool ReadObjectStart()
    {
        return TryReadCharacter('{');
    }

    public bool ReadEndObject()
    {
        return TryReadCharacter('}');
    }

    public bool ReadComma()
    {
        return TryReadCharacter(',');
    }

    public bool ReadPropertyName(string name)
    {
        if (!TryReadCharacter('"'))
            return false;

        for (var i = 0; i < name.Length; i++)
            if (!TryReadCharacter(name[i]))
                return false;

        if (!TryReadCharacter('"'))
            return false;

        if (!TryReadCharacter(':'))
            return false;

        return true;
    }

    public string? ReadString()
    {
        if (!TryReadCharacter('"'))
            return null;

        var start = _index;
        while (_index < _json.Length && _json[_index] != '"')
        {
            if (_json[_index] == '\\')
                _index++; // skip escaped character
            _index++;
        }

        if (_index >= _json.Length)
            return null;

        var value = _json[start.._index];
        _index++; // consume closing quote
        return value;
    }

    public string? ReadUnquotedValue()
    {
        var start = _index;
        while (_index < _json.Length && _json[_index] != ',' && _json[_index] != '}')
            _index++;

        if (_index == start)
            return null;

        return _json[start.._index];
    }
}
