using System.Text;

namespace UnsteamLobbyServer.Protocol;

public abstract record BaseWebsocketMessageToServer
{
    public string Serialize()
    {
        var builder = new StringBuilder();
        var writer = new JsonWriter(builder);

        writer.WriteObjectStart();
        builder.AppendLine();

        writer.WritePropertyName("$type");
        writer.WriteString(GetType().Name);
        writer.WriteComma();
        builder.AppendLine();

        SerializeSelf(ref writer);
        builder.AppendLine();

        writer.WriteObjectEnd();

        return builder.ToString();
    }

    protected abstract void SerializeSelf(ref JsonWriter writer);

    public static BaseWebsocketMessageToServer? Deserialize(Stream json)
    {
        var reader = new JsonReader(json);

        if (!reader.ReadObjectStart()) return null;
        if (!reader.ReadPropertyName("$type")) return null;
        var type = reader.ReadString();
        if (type == null) return null;

        switch (type)
        {
            case nameof(Ping):
                return Ping.DeserializeSelf(ref reader);

            case nameof(CreateLobby):
                return CreateLobby.DeserializeSelf(ref reader);

            case nameof(JoinLobby):
                return JoinLobby.DeserializeSelf(ref reader);

            case nameof(LeaveLobby):
                return LeaveLobby.DeserializeSelf(ref reader);

            default:
                return null;
        }
    }
}

public record Ping(int ID) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WritePropertyName("ID");
        writer.WriteInt32(ID);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadComma())
            return null;

        if (!reader.ReadPropertyName("ID"))
            return null;

        var id = reader.ReadInt32();
        if (!id.HasValue)
            return null;

        return new Ping(id.Value);
    }
}

public record CreateLobby(ulong Owner, LobbyVisibility Visibility, byte MaxMembers) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WritePropertyName("Owner");
        writer.WriteUInt64(Owner);
        writer.WriteComma();
        writer.WritePropertyName("Visibility");
        writer.WriteString(Visibility.ToString());
        writer.WriteComma();
        writer.WritePropertyName("MaxMembers");
        writer.WriteInt32(MaxMembers);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadComma())
            return null;

        if (!reader.ReadPropertyName("Owner"))
            return null;

        var owner = reader.ReadUInt64();
        if (!owner.HasValue)
            return null;

        if (!reader.ReadComma())
            return null;

        if (!reader.ReadPropertyName("Visibility"))
            return null;

        var visibilityStr = reader.ReadString();
        if (visibilityStr == null || !Enum.TryParse<LobbyVisibility>(visibilityStr, out var visibility))
            return null;

        if (!reader.ReadComma())
            return null;

        if (!reader.ReadPropertyName("MaxMembers"))
            return null;

        var maxMembersStr = reader.ReadUnquotedValue();
        if (maxMembersStr == null || !byte.TryParse(maxMembersStr, out var maxMembers))
            return null;

        return new CreateLobby(owner.Value, visibility, maxMembers);
    }
}

public record JoinLobby(ulong LobbyId, ulong UserId) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WritePropertyName("LobbyId");
        writer.WriteUInt64(LobbyId);
        writer.WriteComma();
        writer.WritePropertyName("UserId");
        writer.WriteUInt64(UserId);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadComma())
            return null;

        if (!reader.ReadPropertyName("LobbyId"))
            return null;

        var lobbyId = reader.ReadUInt64();
        if (!lobbyId.HasValue)
            return null;

        if (!reader.ReadComma())
            return null;

        if (!reader.ReadPropertyName("UserId"))
            return null;

        var userId = reader.ReadUInt64();
        if (!userId.HasValue)
            return null;

        return new JoinLobby(lobbyId.Value, userId.Value);
    }
}

public record LeaveLobby(ulong LobbyId, ulong UserId) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WritePropertyName("LobbyId");
        writer.WriteUInt64(LobbyId);
        writer.WriteComma();
        writer.WritePropertyName("UserId");
        writer.WriteUInt64(UserId);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadComma())
            return null;

        if (!reader.ReadPropertyName("LobbyId"))
            return null;

        var lobbyId = reader.ReadUInt64();
        if (!lobbyId.HasValue)
            return null;

        if (!reader.ReadComma())
            return null;

        if (!reader.ReadPropertyName("UserId"))
            return null;

        var userId = reader.ReadUInt64();
        if (!userId.HasValue)
            return null;

        return new LeaveLobby(lobbyId.Value, userId.Value);
    }
}