using HandySerialization.Wrappers;
using UnsteamLobbyServer.Protocol;

namespace UnsteamLobbyServer.Tests;

[TestClass]
public sealed class RoundtripMessageToClient
{
    private static StreamByteReader SerializeToReader(BaseWebsocketMessageToClient packet)
    {
        var ms = new MemoryStream();
        var writer = new StreamByteWriter(ms);
        
        packet.Serialize(ref writer);

        ms.Position = 0;
        return new StreamByteReader(ms);
    }

    [TestMethod]
    public void Pong_Roundtrip()
    {
        var original = new Pong(99);
        
        var reader = SerializeToReader(original);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(ref reader);
        
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void LobbyCreated_Roundtrip()
    {
        var original = new LobbyCreated(5678);

        var reader = SerializeToReader(original);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(ref reader);
        
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void LobbyEnter_Success_Roundtrip()
    {
        var original = new LobbyEnter(
            LobbyId: 333444555UL,
            Success: true,
            new KeyValuePair<string, string>[]
            {
                new("Hello", "World"),
                new("Goodbye", "Planet"),
            },
            new KeyValuePair<(ulong, string), string>[]
            {
                new((1142, "Hello"), "World"),
                new((46345354567, "Goodbye"), "Planet"),
            }
        );

        var reader = SerializeToReader(original);
        var deserialized = (LobbyEnter?)BaseWebsocketMessageToClient.Deserialize(ref reader);
        Assert.IsNotNull(deserialized);
        
        Assert.AreEqual(original.LobbyId, deserialized.LobbyId);
        Assert.AreEqual(original.Success, deserialized.Success);
        CollectionAssert.AreEqual(original.LobbyData.ToArray(), deserialized.LobbyData.ToArray());
        CollectionAssert.AreEqual(original.LobbyMemberData.ToArray(), deserialized.LobbyMemberData.ToArray());
    }

    [TestMethod]
    public void LobbyEnter_Failure_Roundtrip()
    {
        var original = new LobbyEnter(LobbyId: 333444555UL, Success: false, [], []);

        var reader = SerializeToReader(original);
        var deserialized = (LobbyEnter?)BaseWebsocketMessageToClient.Deserialize(ref reader);
        Assert.IsNotNull(deserialized);
        
        Assert.AreEqual(original.LobbyId, deserialized.LobbyId);
        Assert.AreEqual(original.Success, deserialized.Success);
        CollectionAssert.AreEqual(original.LobbyData.ToArray(), deserialized.LobbyData.ToArray());
        CollectionAssert.AreEqual(original.LobbyMemberData.ToArray(), deserialized.LobbyMemberData.ToArray());
    }

    [TestMethod]
    public void LobbyChatMessage_Roundtrip()
    {
        var original = new LobbyChatMessage(99, 88, "hi");

        var reader = SerializeToReader(original);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(ref reader);

        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void LobbyChatUpdate_Roundtrip()
    {
        var original = new LobbyChatUpdate(99, 88, 77, ChatMemberStateChange.Banned);

        var reader = SerializeToReader(original);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(ref reader);

        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void LobbyDataUpdate_Roundtrip()
    {
        var original = new LobbyDataUpdate(99, 88, true, [
            new KeyValuePair<string, string>("A", "B"),
            new KeyValuePair<string, string>("B", "B"),
        ], [
            new KeyValuePair<(ulong, string), string>((1ul, "A"), "B"),
            new KeyValuePair<(ulong, string), string>((2ul, "A"), "C"),
            new KeyValuePair<(ulong, string), string>((3ul, "B"), "A"),
        ]);

        var reader = SerializeToReader(original);
        var deserialized = (LobbyDataUpdate)BaseWebsocketMessageToClient.Deserialize(ref reader)!;

        Assert.AreEqual(original.LobbyId, deserialized.LobbyId);
        Assert.AreEqual(original.UserId, deserialized.UserId);
        Assert.AreEqual(original.Success, deserialized.Success);
        
        CollectionAssert.AreEqual(original.LobbyData.ToArray(), deserialized.LobbyData.ToArray());
        CollectionAssert.AreEqual(original.LobbyMemberData.ToArray(), deserialized.LobbyMemberData.ToArray());
    }
}
