using UnsteamLobbyServer.Protocol;

namespace UnsteamLobbyServer.Tests;

[TestClass]
public sealed class RoundtripMessageToClient
{
    

    // --- Messages to client ---

    [TestMethod]
    public void Pong_Roundtrip()
    {
        var original = new Pong(99);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(original.Serialize().AsMemory());
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void LobbyCreated_Roundtrip()
    {
        var original = new LobbyCreated(5678);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(original.Serialize().AsMemory());
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
        
        var deserialized = (LobbyEnter?)BaseWebsocketMessageToClient.Deserialize(original.Serialize().AsMemory())!;
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
        
        var deserialized = (LobbyEnter?)BaseWebsocketMessageToClient.Deserialize(original.Serialize().AsMemory());
        Assert.IsNotNull(deserialized);
        
        Assert.AreEqual(original.LobbyId, deserialized.LobbyId);
        Assert.AreEqual(original.Success, deserialized.Success);
        CollectionAssert.AreEqual(original.LobbyData.ToArray(), deserialized.LobbyData.ToArray());
        CollectionAssert.AreEqual(original.LobbyMemberData.ToArray(), deserialized.LobbyMemberData.ToArray());
    }
}
