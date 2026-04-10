namespace UnsteamLobbyServer.Protocol;


public readonly struct JsonWriter
{
    private readonly System.Text.StringBuilder _builder;

    public JsonWriter(System.Text.StringBuilder builder)
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

    public void WriteComma()
    {
        _builder.Append(',');
    }

    public void WritePropertyName(string name)
    {
        _builder.Append('"');
        _builder.Append(name);
        _builder.Append("\": ");
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
}
