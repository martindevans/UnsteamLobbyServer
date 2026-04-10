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
        var original = new LobbyEnter(LobbyId: 333444555UL, Success: true);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(original.Serialize().AsMemory());
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void LobbyEnter_Failure_Roundtrip()
    {
        var original = new LobbyEnter(LobbyId: 333444555UL, Success: false);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(original.Serialize().AsMemory());
        Assert.AreEqual(original, deserialized);
    }
}
