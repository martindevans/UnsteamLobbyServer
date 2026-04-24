using HandySerialization;
using HandySerialization.Extensions;
using HandySerialization.Extensions.Collections;

namespace UnsteamLobbyServer.Protocol;

public abstract record BaseWebsocketMessageToClient
{
    public void Serialize<TWriter>(ref TWriter writer)
        where TWriter : struct, IByteWriter
    {
        writer.Write(GetType().Name);
        SerializeSelf(ref writer);
    }

    protected abstract void SerializeSelf<TWriter>(ref TWriter writer)
        where TWriter : struct, IByteWriter;

    public static BaseWebsocketMessageToClient? Deserialize<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        var type = reader.ReadString();

        return type switch
        {
            nameof(Pong) => Pong.DeserializeSelf(ref reader),
            nameof(LobbyCreated) => LobbyCreated.DeserializeSelf(ref reader),
            nameof(LobbyEnter) => LobbyEnter.DeserializeSelf(ref reader),
            nameof(LobbyChatUpdate) => LobbyChatUpdate.DeserializeSelf(ref reader),
            nameof(LobbyDataUpdate) => LobbyDataUpdate.DeserializeSelf(ref reader),
            nameof(LobbyChatMessage) => LobbyChatMessage.DeserializeSelf(ref reader),
            nameof(LobbyList) => LobbyList.DeserializeSelf(ref reader),
            
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
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(ID);
    }

    internal static Pong DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new Pong(
            reader.ReadInt32()
        );
    }
}

/// <summary>
/// Indicates that a lobby with the given ID was created
/// </summary>
/// <param name="LobbyId"></param>
public record LobbyCreated(ulong LobbyId, IReadOnlyDictionary<string, string> Metadata)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(Metadata, new StringAdapter(), new StringAdapter());
    }

    internal static LobbyCreated DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new LobbyCreated(
            reader.ReadUInt64(),
            reader.ReadDictionary<TReader, string, StringAdapter, string, StringAdapter>()
        );
    }
}

/// <summary>
/// Indicates that the user receiving this message has entered a lobby with the given ID
/// </summary>
/// <param name="LobbyId"></param>
public record LobbyEnter(ulong LobbyId, bool Success, IReadOnlyList<KeyValuePair<string, string>> LobbyData, IReadOnlyList<KeyValuePair<(ulong, string), string>> LobbyMemberData)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(Convert.ToByte(Success));
        
        writer.Write(checked((ushort)LobbyData.Count));
        foreach (var (key, value) in LobbyData)
        {
            writer.Write(key);
            writer.Write(value);
        }
        
        writer.Write(checked((ushort)LobbyMemberData.Count));
        foreach (var ((uid, key), value) in LobbyMemberData)
        {
            writer.Write(uid);
            writer.Write(key);
            writer.Write(value);
        }
    }

    internal static LobbyEnter DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        var lobbyId = reader.ReadUInt64();
        var success = Convert.ToBoolean(reader.ReadUInt8());

        var lobbyData = new KeyValuePair<string, string>[reader.ReadUInt16()];
        for (var i = 0; i < lobbyData.Length; i++)
        {
            lobbyData[i] = new KeyValuePair<string, string>(
                reader.ReadString() ?? "",
                reader.ReadString() ?? ""
            );
        }

        var lobbyMemberData = new KeyValuePair<(ulong, string), string>[reader.ReadUInt16()];
        for (var i = 0; i < lobbyMemberData.Length; i++)
        {
            lobbyMemberData[i] = new KeyValuePair<(ulong, string), string>(
                (reader.ReadUInt64(), reader.ReadString() ?? ""),
                reader.ReadString() ?? ""
            );
        }

        return new LobbyEnter(
            lobbyId,
            success,
            lobbyData,
            lobbyMemberData
        );
    }
}

/// <summary>
/// Indicates that a new chat message was sent in the lobby
/// </summary>
/// <param name="LobbyId"></param>
/// <param name="UserId"></param>
/// <param name="Message"></param>
public record LobbyChatMessage(ulong LobbyId, ulong UserId, string Message)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(UserId);
        writer.Write(Message);
    }

    internal static LobbyChatMessage DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new LobbyChatMessage(
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadString() ?? ""
        );
    }
}

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
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(UserChangedId);
        writer.Write(UserMakingChangeId);
        writer.Write((int)State);
    }

    internal static LobbyChatUpdate DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new LobbyChatUpdate(
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            (ChatMemberStateChange)reader.ReadInt32()
        );
    }
}

/// <summary>
/// Indicates that lobby data has updated
/// </summary>
/// <param name="LobbyId"></param>
/// <param name="UserId">Either the ID of the user who the data was updated for, or the lobby ID if the lobby data was updated</param>
/// <param name="Success"></param>
public record LobbyDataUpdate(
    ulong LobbyId,
    ulong UserId,
    bool Success,
    IReadOnlyList<KeyValuePair<string, string>> LobbyData,
    IReadOnlyList<KeyValuePair<(ulong, string), string>> LobbyMemberData,
    int MemberCount,
    int MemberLimit
)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(LobbyId);
        writer.Write(UserId);
        writer.Write(Convert.ToByte(Success));

        writer.Write(checked((ushort)LobbyData.Count));
        foreach (var (key, value) in LobbyData)
        {
            writer.Write(key);
            writer.Write(value);
        }

        writer.Write(checked((ushort)LobbyMemberData.Count));
        foreach (var ((uid, key), value) in LobbyMemberData)
        {
            writer.Write(uid);
            writer.Write(key);
            writer.Write(value);
        }

        writer.Write(MemberCount);
        writer.Write(MemberLimit);
    }

    internal static LobbyDataUpdate DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        var lobbyId = reader.ReadUInt64();
        var userId = reader.ReadUInt64();
        var success = Convert.ToBoolean(reader.ReadUInt8());

        var lobbyData = new KeyValuePair<string, string>[reader.ReadUInt16()];
        for (var i = 0; i < lobbyData.Length; i++)
        {
            lobbyData[i] = new KeyValuePair<string, string>(
                reader.ReadString() ?? "",
                reader.ReadString() ?? ""
            );
        }

        var lobbyMemberData = new KeyValuePair<(ulong, string), string>[reader.ReadUInt16()];
        for (var i = 0; i < lobbyMemberData.Length; i++)
        {
            lobbyMemberData[i] = new KeyValuePair<(ulong, string), string>(
                (reader.ReadUInt64(), reader.ReadString() ?? ""),
                reader.ReadString() ?? ""
            );
        }

        var memberCount = reader.ReadInt32();
        var memberLimit = reader.ReadInt32();

        return new LobbyDataUpdate(
            lobbyId,
            userId,
            success,
            lobbyData,
            lobbyMemberData,
            memberCount,
            memberLimit
        );
    }
}

/// <summary>
/// A single lobby entry in the lobby list
/// </summary>
public record LobbyListEntry(
    ulong LobbyId,
    ulong Owner,
    int MemberCount,
    int MemberLimit,
    LobbyVisibility Visibility,
    IReadOnlyList<KeyValuePair<string, string>> LobbyData
)
{
    internal void Serialize<TWriter>(ref TWriter writer)
        where TWriter : struct, IByteWriter
    {
        writer.Write(LobbyId);
        writer.Write(Owner);
        writer.Write(MemberCount);
        writer.Write(MemberLimit);
        writer.Write((int)Visibility);

        writer.Write(checked((ushort)LobbyData.Count));
        foreach (var (key, value) in LobbyData)
        {
            writer.Write(key);
            writer.Write(value);
        }
    }

    internal static LobbyListEntry Deserialize<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        var lobbyId = reader.ReadUInt64();
        var owner = reader.ReadUInt64();
        var memberCount = reader.ReadInt32();
        var memberLimit = reader.ReadInt32();
        var visibility = (LobbyVisibility)reader.ReadInt32();

        var lobbyData = new KeyValuePair<string, string>[reader.ReadUInt16()];
        for (var i = 0; i < lobbyData.Length; i++)
        {
            lobbyData[i] = new KeyValuePair<string, string>(
                reader.ReadString() ?? "",
                reader.ReadString() ?? ""
            );
        }

        return new LobbyListEntry(lobbyId, owner, memberCount, memberLimit, visibility, lobbyData);
    }
}

/// <summary>
/// Contains a snapshot of all current lobbies and their metadata (excluding per-member data)
/// </summary>
public record LobbyList(IReadOnlyList<LobbyListEntry> Lobbies)
    : BaseWebsocketMessageToClient
{
    protected override void SerializeSelf<TWriter>(ref TWriter writer)
    {
        writer.Write(checked((ushort)Lobbies.Count));
        foreach (var entry in Lobbies)
            entry.Serialize(ref writer);
    }

    internal static LobbyList DeserializeSelf<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        var lobbies = new LobbyListEntry[reader.ReadUInt16()];
        for (var i = 0; i < lobbies.Length; i++)
            lobbies[i] = LobbyListEntry.Deserialize(ref reader);

        return new LobbyList(lobbies);
    }
}