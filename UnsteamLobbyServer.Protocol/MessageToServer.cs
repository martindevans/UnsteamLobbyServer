using HandySerialization;
using HandySerialization.Extensions;

namespace UnsteamLobbyServer.Protocol;

public abstract record BaseWebsocketMessageToServer
{
    public void Serialize<TWriter>(ref TWriter writer)
        where TWriter : struct, IByteWriter
    {
        writer.Write(GetType().Name);
        SerializeSelf(ref writer);
    }

    protected abstract void SerializeSelf<TWriter>(ref TWriter writer)
        where TWriter : struct, IByteWriter;

    public static BaseWebsocketMessageToServer? Deserialize<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        var type = reader.ReadString();

        return type switch
        {
            nameof(Ping) => Ping.DeserializeSelf(ref reader),
            nameof(CreateLobby) => CreateLobby.DeserializeSelf(ref reader),
            nameof(JoinLobby) => JoinLobby.DeserializeSelf(ref reader),
            nameof(LeaveLobby) => LeaveLobby.DeserializeSelf(ref reader),
            nameof(SetLobbyMemberLimit) => SetLobbyMemberLimit.DeserializeSelf(ref reader),
            nameof(SetLobbyVisibility) => SetLobbyVisibility.DeserializeSelf(ref reader),
            nameof(DeleteLobbyData) => DeleteLobbyData.DeserializeSelf(ref reader),
            nameof(SetLobbyData) => SetLobbyData.DeserializeSelf(ref reader),
            nameof(SetLobbyMemberData) => SetLobbyMemberData.DeserializeSelf(ref reader),
            nameof(SetLobbyOwner) => SetLobbyOwner.DeserializeSelf(ref reader),
            nameof(SendLobbyChat) => SendLobbyChat.DeserializeSelf(ref reader),
            
            _ => null
        };
    }
}

public record Ping(int ID)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(ID);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new Ping(
            reader.ReadInt32()
        );
    }
}

public record CreateLobby(ulong Owner, LobbyVisibility Visibility, byte MaxMembers)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(Owner);
        writer.Write((int)Visibility);
        writer.Write(MaxMembers);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new CreateLobby(
            reader.ReadUInt64(),
            (LobbyVisibility)reader.ReadInt32(),
            reader.ReadUInt8()
        );
    }
}

public record JoinLobby(ulong LobbyId, ulong UserId)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(UserId);
    }

    internal static BaseWebsocketMessageToServer DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new JoinLobby(
            reader.ReadUInt64(),
            reader.ReadUInt64()
        );
    }
}

public record LeaveLobby(ulong LobbyId, ulong UserId)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(UserId);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new LeaveLobby(
            reader.ReadUInt64(),
            reader.ReadUInt64()
        );
    }
}

public record SetLobbyMemberLimit(ulong LobbyId, ulong Sender, int MaxMembers)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
       writer.Write(LobbyId);
       writer.Write(Sender);
       writer.Write(MaxMembers);
    }

    internal static BaseWebsocketMessageToServer DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new SetLobbyMemberLimit(
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadInt32()
        );
    }
}

public record SetLobbyVisibility(ulong LobbyId, ulong Sender, LobbyVisibility Visibility)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(Sender);
        writer.Write((int)Visibility);
    }

    internal static BaseWebsocketMessageToServer DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new SetLobbyVisibility(
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            (LobbyVisibility)reader.ReadInt32()
        );
    }
}

public record DeleteLobbyData(ulong LobbyId, ulong Sender, string Key)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(Sender);
        writer.Write(Key);
    }

    internal static BaseWebsocketMessageToServer DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new DeleteLobbyData(
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadString() ?? ""
        );
    }
}

public record SetLobbyData(ulong LobbyId, ulong Sender, string Key, string Value)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(Sender);
        writer.Write(Key);
        writer.Write(Value);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new SetLobbyData(
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadString() ?? "",
            reader.ReadString() ?? ""
        );
    }
}

public record SetLobbyMemberData(ulong LobbyId, ulong Sender, string Key, string Value)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(Sender);
        writer.Write(Key);
        writer.Write(Value);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new SetLobbyMemberData(
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadString() ?? "",
            reader.ReadString() ?? ""
        );
    }
}

public record SetLobbyOwner(ulong LobbyId, ulong Sender, ulong NewOwner)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(Sender);
        writer.Write(NewOwner);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new SetLobbyOwner(
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadUInt64()
        );
    }
}

public record SendLobbyChat(ulong LobbyId, ulong Sender, string Message)
    : BaseWebsocketMessageToServer
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(Sender);
        writer.Write(Message);
    }

    internal static BaseWebsocketMessageToServer? DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new SendLobbyChat(
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadString() ?? ""
        );
    }
}