namespace UnsteamLobbyServer.Protocol.Json;

internal struct JsonReader
{
    private readonly ReadOnlyMemory<char> _json;
    private int _index;

    public JsonReader(ReadOnlyMemory<char> json)
    {
        _json = json;
    }

    private void SkipWhitespace()
    {
        var json = _json.Span;
        
        while (_index < json.Length && char.IsWhiteSpace(json[_index]))
            _index++;
    }

    private bool TryReadCharacter(char character)
    {
        var json = _json.Span;

        if (_index >= json.Length || json[_index] != character)
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

        var json = _json.Span;
        var start = _index;
        while (_index < _json.Length && json[_index] != '"')
        {
            if (json[_index] == '\\')
                _index++; // skip escaped character
            _index++;
        }

        if (_index >= _json.Length)
        {
            value = default!;
            return false;
        }

        value = new string(json[start.._index]);
        _index++; // consume closing quote
        return true;
    }

    public bool ReadUnquotedValue(out string value)
    {
        SkipWhitespace();

        var json = _json.Span;
        var start = _index;
        while (_index < _json.Length && json[_index] != ',' && json[_index] != '}' && json[_index] != ' ')
            _index++;

        if (_index == start)
        {
            value = "";
            return true;
        }

        value = new string(json[start.._index]).TrimEnd();
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


    public bool ReadPropertyUInt64(string name, out ulong value)
    {
        if (!ReadPropertyName(name))
        {
            value = default;
            return false;
        }

        if (!ReadUInt64(out value))
            return false;
        
        if (!ReadComma())
            return false;

        return true;
    }

    public bool ReadPropertyInt32(string name, out int value)
    {
        if (!ReadPropertyName(name))
        {
            value = default;
            return false;
        }

        if (!ReadInt32(out value))
            return false;

        if (!ReadComma())
            return false;

        return true;
    }

    public bool ReadPropertyUInt8(string name, out byte value)
    {
        if (!ReadPropertyName(name))
        {
            value = default;
            return false;
        }

        if (!ReadByte(out value))
            return false;

        if (!ReadComma())
            return false;

        return true;
    }

    public bool ReadPropertyBool(string name, out bool value)
    {
        if (!ReadPropertyName(name))
        {
            value = default;
            return false;
        }

        if (!ReadBool(out value))
            return false;

        if (!ReadComma())
            return false;

        return true;
    }

    public bool ReadPropertyString(string name, out string value)
    {
        if (!ReadPropertyName(name))
        {
            value = default!;
            return false;
        }

        if (!ReadString(out value))
            return false;

        if (!ReadComma())
            return false;

        return true;
    }
}