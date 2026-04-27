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
        var original = new LobbyCreated(5678, new Dictionary<string, string>());

        var reader = SerializeToReader(original);
        var deserialized = (LobbyCreated)BaseWebsocketMessageToClient.Deserialize(ref reader)!;
        
        Assert.AreEqual(original.LobbyId, deserialized.LobbyId);
    }

    [TestMethod]
    public void LobbyEnter_Success_Roundtrip()
    {
        var original = new LobbyEnter(
            LobbyId: 333444555UL,
            Success: true,
            LobbyData: new KeyValuePair<string, string>[]
            {
                new("Hello", "World"),
                new("Goodbye", "Planet"),
            },
            LobbyMemberData: new KeyValuePair<(ulong, string), string>[]
            {
                new((1142, "Hello"), "World"),
                new((46345354567, "Goodbye"), "Planet"),
            },
            new List<ulong>()
            {
                3432876,
                45698097
            }
        );

        var reader = SerializeToReader(original);
        var deserialized = (LobbyEnter?)BaseWebsocketMessageToClient.Deserialize(ref reader);
        Assert.IsNotNull(deserialized);
        
        Assert.AreEqual(original.LobbyId, deserialized.LobbyId);
        Assert.AreEqual(original.Success, deserialized.Success);
        CollectionAssert.AreEqual(original.LobbyData.ToArray(), deserialized.LobbyData.ToArray());
        CollectionAssert.AreEqual(original.LobbyMemberData.ToArray(), deserialized.LobbyMemberData.ToArray());
        CollectionAssert.AreEqual(original.LobbyMembers.ToArray(), deserialized.LobbyMembers.ToArray());
    }

    [TestMethod]
    public void LobbyEnter_Failure_Roundtrip()
    {
        var original = new LobbyEnter(LobbyId: 333444555UL, Success: false, LobbyData: [], LobbyMemberData: [], LobbyMembers: []);

        var reader = SerializeToReader(original);
        var deserialized = (LobbyEnter?)BaseWebsocketMessageToClient.Deserialize(ref reader);
        Assert.IsNotNull(deserialized);
        
        Assert.AreEqual(original.LobbyId, deserialized.LobbyId);
        Assert.AreEqual(original.Success, deserialized.Success);
        CollectionAssert.AreEqual(original.LobbyData.ToArray(), deserialized.LobbyData.ToArray());
        CollectionAssert.AreEqual(original.LobbyMemberData.ToArray(), deserialized.LobbyMemberData.ToArray());
        CollectionAssert.AreEqual(original.LobbyMembers.ToArray(), deserialized.LobbyMembers.ToArray());
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
        ], 3, 10);

        var reader = SerializeToReader(original);
        var deserialized = (LobbyDataUpdate)BaseWebsocketMessageToClient.Deserialize(ref reader)!;

        Assert.AreEqual(original.LobbyId, deserialized.LobbyId);
        Assert.AreEqual(original.UserId, deserialized.UserId);
        Assert.AreEqual(original.Success, deserialized.Success);
        
        CollectionAssert.AreEqual(original.LobbyData.ToArray(), deserialized.LobbyData.ToArray());
        CollectionAssert.AreEqual(original.LobbyMemberData.ToArray(), deserialized.LobbyMemberData.ToArray());
    }

    [TestMethod]
    public void LobbyList_Roundtrip()
    {
        var original = new LobbyList([
            new LobbyListEntry(
                LobbyId: 111222333UL,
                Owner: 999888777UL,
                MemberCount: 2,
                MemberLimit: 8,
                Visibility: LobbyVisibility.Public,
                LobbyData: [
                    new KeyValuePair<string, string>("map", "dust2"),
                    new KeyValuePair<string, string>("mode", "tdm"),
                ]
            ),
            new LobbyListEntry(
                LobbyId: 444555666UL,
                Owner: 123456789UL,
                MemberCount: 1,
                MemberLimit: 4,
                Visibility: LobbyVisibility.Private,
                LobbyData: []
            ),
        ]);

        var reader = SerializeToReader(original);
        var deserialized = (LobbyList?)BaseWebsocketMessageToClient.Deserialize(ref reader);
        Assert.IsNotNull(deserialized);

        Assert.HasCount(original.Lobbies.Count, deserialized.Lobbies);
        for (var i = 0; i < original.Lobbies.Count; i++)
        {
            var o = original.Lobbies[i];
            var d = deserialized.Lobbies[i];
            Assert.AreEqual(o.LobbyId, d.LobbyId);
            Assert.AreEqual(o.Owner, d.Owner);
            Assert.AreEqual(o.MemberCount, d.MemberCount);
            Assert.AreEqual(o.MemberLimit, d.MemberLimit);
            Assert.AreEqual(o.Visibility, d.Visibility);
            CollectionAssert.AreEqual(o.LobbyData.ToArray(), d.LobbyData.ToArray());
        }
    }
}
