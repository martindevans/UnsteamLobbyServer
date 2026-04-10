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

        if (!reader.ReadObjectStart())
            return null;
        if (!reader.ReadPropertyName("$type"))
            return null;
        if (!reader.ReadString(out var type))
            return null;

        if (!reader.ReadComma())
            return null;

        return type switch
        {
            nameof(Ping) => Ping.DeserializeSelf(ref reader),
            nameof(CreateLobby) => CreateLobby.DeserializeSelf(ref reader),
            nameof(JoinLobby) => JoinLobby.DeserializeSelf(ref reader),
            nameof(LeaveLobby) => LeaveLobby.DeserializeSelf(ref reader),
            _ => null
        };
    }
}

public record Ping(int ID)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WritePropertyName("ID");
        writer.WriteInt32(ID);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyName("ID")
         || !reader.ReadInt32(out var id))
            return null;

        return new Ping(id);
    }
}

public record CreateLobby(ulong Owner, LobbyVisibility Visibility, byte MaxMembers)
    : BaseWebsocketMessageToServer
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
        if (!reader.ReadPropertyName("Owner")
         || !reader.ReadUInt64(out var owner)
         || !reader.ReadComma()
         || !reader.ReadPropertyName("Visibility")
         || !reader.ReadString(out var visibilityStr)
         || !Enum.TryParse<LobbyVisibility>(visibilityStr, out var visibility)
         || !reader.ReadComma()
         || !reader.ReadPropertyName("MaxMembers")
         || !reader.ReadByte(out var maxMembers))
             return null;

        return new CreateLobby(owner, visibility, maxMembers);
    }
}

public record JoinLobby(ulong LobbyId, ulong UserId)
    : BaseWebsocketMessageToServer
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
        if (!reader.ReadPropertyName("LobbyId")
         || !reader.ReadUInt64(out var lobbyId)
         || !reader.ReadComma()
         || !reader.ReadPropertyName("UserId")
         || !reader.ReadUInt64(out var userId))
             return null;

        return new JoinLobby(lobbyId, userId);
    }
}

public record LeaveLobby(ulong LobbyId, ulong UserId)
    : BaseWebsocketMessageToServer
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
        if (!reader.ReadPropertyName("LobbyId")
         || !reader.ReadUInt64(out var lobbyId)
         || !reader.ReadComma()
         || !reader.ReadPropertyName("UserId")
         || !reader.ReadUInt64(out var userId))
             return null;

        return new LeaveLobby(lobbyId, userId);
    }
}