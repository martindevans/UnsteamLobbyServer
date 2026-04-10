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

    public JsonReader(Stream stream)
    {
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
        _json = reader.ReadToEnd();
    }

    private void SkipWhitespace()
    {
        while (_index < _json.Length && char.IsWhiteSpace(_json[_index]))
            _index++;
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
        SkipWhitespace();
        return TryReadCharacter('{');
    }

    public bool ReadEndObject()
    {
        SkipWhitespace();
        return TryReadCharacter('}');
    }

    public bool ReadComma()
    {
        SkipWhitespace();
        return TryReadCharacter(',');
    }

    public bool ReadPropertyName(string name)
    {
        SkipWhitespace();
        if (!TryReadCharacter('"'))
            return false;

        for (var i = 0; i < name.Length; i++)
            if (!TryReadCharacter(name[i]))
                return false;

        if (!TryReadCharacter('"'))
            return false;

        SkipWhitespace();
        if (!TryReadCharacter(':'))
            return false;

        return true;
    }

    public string? ReadString()
    {
        SkipWhitespace();
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
        SkipWhitespace();
        var start = _index;
        while (_index < _json.Length && _json[_index] != ',' && _json[_index] != '}')
            _index++;

        if (_index == start)
            return null;

        return _json[start.._index].TrimEnd();
    }

    public int? ReadInt32()
    {
        var str = ReadUnquotedValue();
        if (str == null || !int.TryParse(str, out var value))
            return null;

        return value;
    }
}
