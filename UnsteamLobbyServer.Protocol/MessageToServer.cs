using System.Text;
using UnsteamLobbyServer.Protocol.Json;

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

    public static BaseWebsocketMessageToServer? Deserialize(ReadOnlyMemory<char> json)
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
            nameof(SetLobbyMemberLimit) => SetLobbyMemberLimit.DeserializeSelf(ref reader),
            _ => null
        };
    }
}

public record Ping(int ID)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WriteProperty(nameof(ID), ID);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyInt32(nameof(ID), out var id))
            return null;

        return new Ping(id);
    }
}

public record CreateLobby(ulong Owner, LobbyVisibility Visibility, byte MaxMembers)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WriteProperty(nameof(Owner), Owner);
        writer.WriteProperty(nameof(Visibility), (int)Visibility);
        writer.WriteProperty(nameof(MaxMembers), MaxMembers);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyUInt64(nameof(Owner), out var owner))
            return null;
        if (!reader.ReadPropertyInt32(nameof(Visibility), out var visibility))
            return null;
        if (!reader.ReadPropertyUInt8(nameof(MaxMembers), out var maxMembers))
            return null;

        return new CreateLobby(owner, (LobbyVisibility)visibility, maxMembers);
    }
}

public record JoinLobby(ulong LobbyId, ulong UserId)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WriteProperty(nameof(LobbyId), LobbyId);
        writer.WriteProperty(nameof(UserId), UserId);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyUInt64(nameof(LobbyId), out var lobbyId))
            return null;
        if (!reader.ReadPropertyUInt64(nameof(UserId), out var userId))
            return null;

        return new JoinLobby(lobbyId, userId);
    }
}

public record LeaveLobby(ulong LobbyId, ulong UserId)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WriteProperty(nameof(LobbyId), LobbyId);
        writer.WriteProperty(nameof(UserId), UserId);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyUInt64(nameof(LobbyId), out var lobbyId))
            return null;
        if (!reader.ReadPropertyUInt64(nameof(UserId), out var userId))
            return null;

        return new LeaveLobby(lobbyId, userId);
    }
}

public record SetLobbyMemberLimit(ulong LobbyId, ulong Sender, int MaxMembers)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WriteProperty("LobbyId", LobbyId);
        writer.WriteProperty("Sender", Sender);
        writer.WriteProperty("MaxMembers", MaxMembers);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyUInt64("LobbyId", out var lobbyId))
            return null;
        if (!reader.ReadPropertyUInt64("Sender", out var senderId))
            return null;
        if (!reader.ReadPropertyInt32("MaxMembers", out var maxMembers))
            return null;

        return new SetLobbyMemberLimit(lobbyId, senderId, maxMembers);
    }
}