using JetBrains.Annotations;
using System.Text;
using UnsteamLobbyServer.Protocol.Json;

namespace UnsteamLobbyServer.Protocol;

[UsedImplicitly]
public abstract record BaseWebsocketMessageToClient
{
    public string Serialize()
    {
        var builder = new StringBuilder();
        var writer = new JsonWriter(builder);

        writer.WriteObjectStart();
        {
            writer.WritePropertyName("$type");
            writer.WriteString(GetType().Name);
            writer.WriteComma();

            SerializeSelf(ref writer);
        }
        writer.WriteObjectEnd();

        return builder.ToString();
    }

    protected abstract void SerializeSelf(ref JsonWriter writer);

    public static BaseWebsocketMessageToClient? Deserialize(ReadOnlyMemory<char> json)
    {
        var reader = new JsonReader(json);

        // {
        if (!reader.ReadObjectStart())
            return null;
        
        // "$type": "whatever",
        if (!reader.ReadPropertyName("$type"))
            return null;
        if (!reader.ReadString(out var type))
            return null;
        if (!reader.ReadComma())
            return null;

        return type switch
        {
            nameof(Pong) => Pong.DeserializeSelf(ref reader),
            nameof(LobbyCreated) => LobbyCreated.DeserializeSelf(ref reader),
            nameof(LobbyEnter) => LobbyEnter.DeserializeSelf(ref reader),
            nameof(LobbyChatUpdate) => LobbyChatUpdate.DeserializeSelf(ref reader),
            nameof(LobbyDataUpdate) => LobbyDataUpdate.DeserializeSelf(ref reader),
            _ => null
        };
    }
}

/// <summary>
/// Response to a Ping message
/// </summary>
/// <param name="ID"></param>
public record Pong(int ID)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WriteProperty(nameof(ID), ID);
    }

    internal static BaseWebsocketMessageToClient? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyInt32(nameof(ID), out var id))
            return null;
        
        return new Pong(id);
    }
}

/// <summary>
/// Indicates that a lobby with the given ID was created
/// </summary>
/// <param name="LobbyId"></param>
public record LobbyCreated(ulong LobbyId)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WriteProperty(nameof(LobbyId), LobbyId);
    }

    internal static BaseWebsocketMessageToClient? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyUInt64(nameof(LobbyId), out var lobbyId))
             return null;

        return new LobbyCreated(lobbyId);
    }
}

/// <summary>
/// Indicates that the user receiving this message has entered a lobby with the given ID
/// </summary>
/// <param name="LobbyId"></param>
public record LobbyEnter(ulong LobbyId, bool Success, IReadOnlyList<KeyValuePair<string, string>> LobbyData, IReadOnlyList<KeyValuePair<(ulong, string), string>> LobbyMemberData)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WriteProperty(nameof(LobbyId), LobbyId);
        writer.WriteProperty(nameof(Success), Success);
        
        writer.WriteProperty(nameof(LobbyData), LobbyData);
        writer.WriteProperty(nameof(LobbyMemberData), LobbyMemberData);
    }

    internal static BaseWebsocketMessageToClient? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyUInt64(nameof(LobbyId), out var lobbyId))
            return null;
        if (!reader.ReadPropertyBool(nameof(Success), out var success))
            return null;
        if (!reader.ReadPropertyLobbyData(nameof(LobbyData), out var ldata))
            return null;
        if (!reader.ReadPropertyLobbyMemberData(nameof(LobbyMemberData), out var lmdata))
            return null;

        return new LobbyEnter(lobbyId, success, ldata, lmdata);
    }
}

///// <summary>
///// Indicates that a new chat message was sent in the lobby
///// </summary>
///// <param name="LobbyId"></param>
///// <param name="UserId"></param>
///// <param name="Type"></param>
///// <param name="ChatId"></param>
//public record LobbyChatMessage(ulong LobbyId, ulong UserId, ChatType Type, uint ChatId, byte[] Data) : BaseWebsocketMessageToClient;

/// <summary>
/// Indicates that soemthing about the lobby chat has updated (e.g. user joined or left)
/// </summary>
/// <param name="LobbyId"></param>
/// <param name="UserChangedId"></param>
/// <param name="UserMakingChangeId"></param>
/// <param name="State"></param>
public record LobbyChatUpdate(ulong LobbyId, ulong UserChangedId, ulong UserMakingChangeId, ChatMemberStateChange State)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WriteProperty(nameof(LobbyId), LobbyId);
        writer.WriteProperty(nameof(UserChangedId), UserChangedId);
        writer.WriteProperty(nameof(UserMakingChangeId), UserMakingChangeId);
        writer.WriteProperty(nameof(State), (int)State);
    }

    internal static BaseWebsocketMessageToClient? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyUInt64(nameof(LobbyId), out var lobbyId))
            return null;
        if (!reader.ReadPropertyUInt64(nameof(UserChangedId), out var userChanged))
            return null;
        if (!reader.ReadPropertyUInt64(nameof(UserMakingChangeId), out var userMakingChange))
            return null;
        if (!reader.ReadPropertyInt32(nameof(UserMakingChangeId), out var state))
            return null;

        return new LobbyChatUpdate(lobbyId, userChanged, userMakingChange, (ChatMemberStateChange)state);
    }
}

/// <summary>
/// Indicates that lobby data has updated
/// </summary>
/// <param name="LobbyId"></param>
/// <param name="UserId">Either the ID of the user who the data was updated for, or the lobby ID if the lobby data was updated</param>
/// <param name="Success"></param>
public record LobbyDataUpdate(ulong LobbyId, ulong UserId, bool Success)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf(ref JsonWriter writer)
    {
        writer.WriteProperty(nameof(LobbyId), LobbyId);
        writer.WriteProperty(nameof(UserId), UserId);
        writer.WriteProperty(nameof(Success), Success);
    }

    internal static BaseWebsocketMessageToClient? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadPropertyUInt64(nameof(LobbyId), out var lobbyId))
            return null;
        if (!reader.ReadPropertyUInt64(nameof(UserId), out var userId))
            return null;
        if (!reader.ReadPropertyBool(nameof(Success), out var success))
            return null;

        return new LobbyDataUpdate(lobbyId, userId, success);
    }
}