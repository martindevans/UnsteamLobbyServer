using System.Text;

namespace UnsteamLobbyServer.Protocol.Json;

public readonly struct JsonWriter
{
    private readonly StringBuilder _builder;

    public JsonWriter(StringBuilder builder)
    {
        _builder = builder;
    }

    public void WriteObjectStart()
    {
        _builder.Append('{');
    }

    public void WriteObjectEnd()
    {
        _builder.Append('}');
    }

    public void WriteArrayStart()
    {
        _builder.Append('[');
    }

    public void WriteArrayEnd()
    {
        _builder.Append(']');
    }

    public void WriteComma()
    {
        _builder.Append(',');
    }

    public void WritePropertyName(string name)
    {
        _builder.Append('"');
        _builder.Append(name);
        _builder.Append("\":");
    }

    public void WriteString(string value)
    {
        _builder.Append('"');
        foreach (var c in value)
        {
            if (c is '"' or '\\')
                _builder.Append('\\');
            _builder.Append(c);
        }
        _builder.Append('"');
    }

    public void WriteUInt8(byte value)
    {
        _builder.Append(value);
    }

    public void WriteInt32(int value)
    {
        _builder.Append(value);
    }

    public void WriteUInt64(ulong value)
    {
        _builder.Append(value);
    }

    public void WriteBool(bool value)
    {
        _builder.Append(value ? "true" : "false");
    }


    public void WriteProperty(string name, string value)
    {
        WritePropertyName(name);
        WriteString(value);
        WriteComma();
    }

    public void WriteProperty(string name, byte value)
    {
        WritePropertyName(name);
        WriteUInt8(value);
        WriteComma();
    }

    public void WriteProperty(string name, int value)
    {
        WritePropertyName(name);
        WriteInt32(value);
        WriteComma();
    }

    public void WriteProperty(string name, ulong value)
    {
        WritePropertyName(name);
        WriteUInt64(value);
        WriteComma();
    }

    public void WriteProperty(string name, bool value)
    {
        WritePropertyName(name);
        WriteBool(value);
        WriteComma();
    }

    public void WriteProperty(string name, IReadOnlyList<KeyValuePair<string, string>> values)
    {
        WritePropertyName(name);
        WriteObjectStart();
        {
            foreach (var (key, value) in values)
                WriteProperty(key, value);
        }
        WriteObjectEnd();
        WriteComma();
    }

    public void WriteProperty(string name, IReadOnlyList<KeyValuePair<(ulong, string), string>> values)
    {
        // name: [
        //   [ uid, key, val ],
        //   [ uid, key, val ],
        //   [ uid, key, val ],
        //   etc
        // ]

        WritePropertyName(name);
        WriteArrayStart();
        {
            foreach (var ((uid, key), value) in values)
            {
                WriteArrayStart();
                WriteUInt64(uid);
                WriteComma();
                WriteString(key);
                WriteComma();
                WriteString(value);
                WriteArrayEnd();
            }
        }
        WriteArrayEnd();
        WriteComma();
    }
}
