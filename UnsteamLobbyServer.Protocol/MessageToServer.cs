using System.Text;

namespace UnsteamLobbyServer.Protocol;

public abstract record BaseWebsocketMessageToServer
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
            {
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("ID")) return null;
                var idStr = reader.ReadUnquotedValue();
                if (idStr == null || !int.TryParse(idStr, out var id)) return null;
                return new Ping(id);
            }

            case nameof(CreateLobby):
            {
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("Owner")) return null;
                var ownerStr = reader.ReadUnquotedValue();
                if (ownerStr == null || !ulong.TryParse(ownerStr, out var owner)) return null;
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("Visibility")) return null;
                var visibilityStr = reader.ReadString();
                if (visibilityStr == null || !Enum.TryParse<LobbyVisibility>(visibilityStr, out var visibility)) return null;
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("MaxMembers")) return null;
                var maxMembersStr = reader.ReadUnquotedValue();
                if (maxMembersStr == null || !byte.TryParse(maxMembersStr, out var maxMembers)) return null;
                return new CreateLobby(owner, visibility, maxMembers);
            }

            case nameof(JoinLobby):
            {
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("LobbyId")) return null;
                var lobbyIdStr = reader.ReadUnquotedValue();
                if (lobbyIdStr == null || !ulong.TryParse(lobbyIdStr, out var lobbyId)) return null;
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("UserId")) return null;
                var userIdStr = reader.ReadUnquotedValue();
                if (userIdStr == null || !ulong.TryParse(userIdStr, out var userId)) return null;
                return new JoinLobby(lobbyId, userId);
            }

            case nameof(LeaveLobby):
            {
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("LobbyId")) return null;
                var lobbyIdStr = reader.ReadUnquotedValue();
                if (lobbyIdStr == null || !ulong.TryParse(lobbyIdStr, out var lobbyId)) return null;
                if (!reader.ReadComma()) return null;
                if (!reader.ReadPropertyName("UserId")) return null;
                var userIdStr = reader.ReadUnquotedValue();
                if (userIdStr == null || !ulong.TryParse(userIdStr, out var userId)) return null;
                return new LeaveLobby(lobbyId, userId);
            }

            default:
                return null;
        }
    }
}

public record Ping(int ID) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"ID\": {0}", ID);
    }
}

public record CreateLobby(ulong Owner, LobbyVisibility Visibility, byte MaxMembers) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"Owner\": {0},", Owner);
        builder.AppendLine();
        builder.AppendFormat("\"Visibility\": \"{0}\",", Visibility);
        builder.AppendLine();
        builder.AppendFormat("\"MaxMembers\": {0}", MaxMembers);
    }
}

public record JoinLobby(ulong LobbyId, ulong UserId) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"LobbyId\": {0},", LobbyId);
        builder.AppendLine();
        builder.AppendFormat("\"UserId\": {0}", UserId);
    }
}

public record LeaveLobby(ulong LobbyId, ulong UserId) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"LobbyId\": {0},", LobbyId);
        builder.AppendLine();
        builder.AppendFormat("\"UserId\": {0}", UserId);
    }
}