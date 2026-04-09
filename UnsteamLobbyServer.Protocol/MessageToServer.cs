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
        var fields = JsonHelper.ParseFields(json);

        if (!fields.TryGetValue("$type", out var type))
            return null;

        return type switch
        {
            nameof(Ping) => new Ping(int.Parse(fields["ID"])),
            nameof(CreateLobby) => new CreateLobby(
                ulong.Parse(fields["Owner"]),
                System.Enum.Parse<LobbyVisibility>(fields["Visibility"]),
                byte.Parse(fields["MaxMembers"])),
            nameof(JoinLobby) => new JoinLobby(
                ulong.Parse(fields["LobbyId"]),
                ulong.Parse(fields["UserId"])),
            nameof(LeaveLobby) => new LeaveLobby(
                ulong.Parse(fields["LobbyId"]),
                ulong.Parse(fields["UserId"])),
            _ => null
        };
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