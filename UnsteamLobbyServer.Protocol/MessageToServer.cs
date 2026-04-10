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
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"ID\": {0}", ID);
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
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"Owner\": {0},", Owner);
        builder.AppendLine();
        builder.AppendFormat("\"Visibility\": \"{0}\",", Visibility);
        builder.AppendLine();
        builder.AppendFormat("\"MaxMembers\": {0}", MaxMembers);
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
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"LobbyId\": {0},", LobbyId);
        builder.AppendLine();
        builder.AppendFormat("\"UserId\": {0}", UserId);
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
    protected override void SerializeSelf(StringBuilder builder)
    {
        builder.AppendFormat("\"LobbyId\": {0},", LobbyId);
        builder.AppendLine();
        builder.AppendFormat("\"UserId\": {0}", UserId);
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