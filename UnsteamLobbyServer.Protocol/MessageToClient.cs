using JetBrains.Annotations;
using System.Text;

namespace UnsteamLobbyServer.Protocol;

[UsedImplicitly]
public abstract record BaseWebsocketMessageToClient
{
    public string Serialize()
    {
        var builder = new StringBuilder();
        builder.AppendLine("{");
        
        builder.AppendFormat("\"$type\": \"{0}\",", GetType().Name);
        builder.AppendLine();

        SerializeSelf(builder);
        builder.AppendLine();

        builder.Append("}");

        return builder.ToString();
    }

    protected abstract void SerializeSelf(StringBuilder builder);

    public static BaseWebsocketMessageToClient? Deserialize(Stream json)
    {
        var reader = new JsonReader(json);

        // {
        if (!reader.ReadObjectStart())
            return null;
        
        // "$type": "whatever"
        if (!reader.ReadPropertyName("$type"))
            return null;
        var type = reader.ReadString();
        if (type == null)
            return null;

        switch (type)
        {
            case nameof(Pong):
                return Pong.DeserializeSelf(ref reader);

            case nameof(LobbyCreated):
            {
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("LobbyId")) return null;
                var lobbyIdStr = reader.ReadUnquotedValue();
                if (lobbyIdStr == null || !ulong.TryParse(lobbyIdStr, out var lobbyId)) return null;
                return new LobbyCreated(lobbyId);
            }

            case nameof(LobbyEnter):
            {
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("LobbyId")) return null;
                var lobbyIdStr = reader.ReadUnquotedValue();
                if (lobbyIdStr == null || !ulong.TryParse(lobbyIdStr, out var lobbyId)) return null;
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("Success")) return null;
                var successStr = reader.ReadUnquotedValue();
                if (successStr == null || !bool.TryParse(successStr, out var success)) return null;
                return new LobbyEnter(lobbyId, success);
            }

            default:
                return null;
        }
    }
}

/// <summary>
/// Response to a Ping message
/// </summary>
/// <param name="ID"></param>
public record Pong(int ID)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"ID\": {0}", ID);
    }

    internal static BaseWebsocketMessageToClient? DeserializeSelf(ref JsonReader reader)
    {
        if (!reader.ReadComma())
            return null;
        
        if (!reader.ReadPropertyName("ID"))
            return null;
        
        var id = reader.ReadInt32();
        if (!id.HasValue)
            return null;
        
        return new Pong(id.Value);
    }
}

/// <summary>
/// Indicates that a lobby with the given ID was created
/// </summary>
/// <param name="LobbyId"></param>
public record LobbyCreated(ulong LobbyId) : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"LobbyId\": {0}", LobbyId);
    }
}

/// <summary>
/// Indicates that the user receiving this message has entered a lobby with the given ID
/// </summary>
/// <param name="LobbyId"></param>
public record LobbyEnter(ulong LobbyId, bool Success) : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"LobbyId\": {0},", LobbyId);
        builder.AppendLine();
        builder.AppendFormat("\"Success\": {0}", Success ? "true" : "false");
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

///// <summary>
///// Indicates that soemthing about the lobby chat has updated (e.g. user joined or left)
///// </summary>
///// <param name="LobbyId"></param>
///// <param name="UserChangedId"></param>
///// <param name="UserMakingChangeId"></param>
///// <param name="State"></param>
//public record LobbyChatUpdate(ulong LobbyId, ulong UserChangedId, ulong UserMakingChangeId, ChatMemberStateChange State) : BaseWebsocketMessageToClient;

///// <summary>
///// Indicates that lobby data has updated
///// </summary>
///// <param name="LobbyId"></param>
///// <param name="UserId">Either the ID of the user who the data was updated for, or the lobby ID if the lobby data was updated</param>
///// <param name="Success"></param>
//public record LobbyDataUpdate(ulong LobbyId, ulong UserId, bool Success) : BaseWebsocketMessageToClient;