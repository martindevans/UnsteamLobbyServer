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
        throw new NotImplementedException();
    }
}

public record Ping(int ID) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        throw new NotImplementedException();
    }
}

public record CreateLobby(ulong Owner, LobbyVisibility Visibility, byte MaxMembers) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        throw new NotImplementedException();
    }
}

public record JoinLobby(ulong LobbyId, ulong UserId) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        throw new NotImplementedException();
    }
}

public record LeaveLobby(ulong LobbyId, ulong UserId) : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf(StringBuilder builder)
    {
        throw new NotImplementedException();
    }
}