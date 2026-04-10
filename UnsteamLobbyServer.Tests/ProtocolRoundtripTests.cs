using System.Text;
using UnsteamLobbyServer.Protocol;

namespace UnsteamLobbyServer.Tests;

[TestClass]
public sealed class ProtocolRoundtripTests
{
    private static Stream ToStream(string s) =>
        new MemoryStream(Encoding.UTF8.GetBytes(s));

    // --- Messages to server ---

    [TestMethod]
    public void Ping_Roundtrip()
    {
        var original = new Ping(42);
        var deserialized = BaseWebsocketMessageToServer.Deserialize(ToStream(original.Serialize()));
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void CreateLobby_Public_Roundtrip()
    {
        var original = new CreateLobby(123456789UL, LobbyVisibility.Public, 8);
        var deserialized = BaseWebsocketMessageToServer.Deserialize(ToStream(original.Serialize()));
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void CreateLobby_Private_Roundtrip()
    {
        var original = new CreateLobby(987654321UL, LobbyVisibility.Private, 2);
        var deserialized = BaseWebsocketMessageToServer.Deserialize(ToStream(original.Serialize()));
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void CreateLobby_FriendsOnly_Roundtrip()
    {
        var original = new CreateLobby(111222333UL, LobbyVisibility.FriendsOnly, 4);
        var deserialized = BaseWebsocketMessageToServer.Deserialize(ToStream(original.Serialize()));
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void JoinLobby_Roundtrip()
    {
        var original = new JoinLobby(LobbyId: 555666777UL, UserId: 111222333UL);
        var deserialized = BaseWebsocketMessageToServer.Deserialize(ToStream(original.Serialize()));
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void LeaveLobby_Roundtrip()
    {
        var original = new LeaveLobby(LobbyId: 999000111UL, UserId: 444555666UL);
        var deserialized = BaseWebsocketMessageToServer.Deserialize(ToStream(original.Serialize()));
        Assert.AreEqual(original, deserialized);
    }

    // --- Messages to client ---

    [TestMethod]
    public void Pong_Roundtrip()
    {
        var original = new Pong(99);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(ToStream(original.Serialize()));
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void LobbyCreated_Roundtrip()
    {
        var original = new LobbyCreated(888777666UL);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(ToStream(original.Serialize()));
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void LobbyEnter_Success_Roundtrip()
    {
        var original = new LobbyEnter(LobbyId: 333444555UL, Success: true);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(ToStream(original.Serialize()));
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void LobbyEnter_Failure_Roundtrip()
    {
        var original = new LobbyEnter(LobbyId: 333444555UL, Success: false);
        var deserialized = BaseWebsocketMessageToClient.Deserialize(ToStream(original.Serialize()));
        Assert.AreEqual(original, deserialized);
    }
}
