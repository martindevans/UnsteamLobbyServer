namespace UnsteamLobbyServer.Protocol;

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

    public bool ReadString(out string value)
    {
        SkipWhitespace();
        if (!TryReadCharacter('"'))
        {
            value = default!;
            return false;
        }

        var start = _index;
        while (_index < _json.Length && _json[_index] != '"')
        {
            if (_json[_index] == '\\')
                _index++; // skip escaped character
            _index++;
        }

        if (_index >= _json.Length)
        {
            value = default!;
            return false;
        }

        value = _json[start.._index];
        _index++; // consume closing quote
        return true;
    }

    public bool ReadUnquotedValue(out string value)
    {
        SkipWhitespace();
        
        var start = _index;
        while (_index < _json.Length && _json[_index] != ',' && _json[_index] != '}' && _json[_index] != ' ')
            _index++;

        if (_index == start)
        {
            value = "";
            return true;
        }

        value = _json[start.._index].TrimEnd();
        return true;
    }

    public bool ReadByte(out byte value)
    {
        if (!ReadUnquotedValue(out var str) || !byte.TryParse(str, out value))
        {
            value = default;
            return false;
        }

        return true;
    }

    public bool ReadInt32(out int value)
    {
        if (!ReadUnquotedValue(out var str) || !int.TryParse(str, out value))
        {
            value = default;
            return false;
        }

        return true;
    }

    public bool ReadUInt64(out ulong value)
    {
        if (!ReadUnquotedValue(out var str) || !ulong.TryParse(str, out value))
        {
            value = default;
            return false;
        }

        return true;
    }

    public bool ReadBool(out bool value)
    {
        if (!ReadUnquotedValue(out var str) || !bool.TryParse(str, out value))
        {
            value = default;
            return false;
        }

        return true;
    }
}