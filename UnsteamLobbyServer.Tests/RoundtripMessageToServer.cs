using UnsteamLobbyServer.Protocol;

namespace UnsteamLobbyServer.Tests
{
    [TestClass]
    public class RoundtripMessageToServer
    {
        [TestMethod]
        public void Ping_Roundtrip()
        {
            var original = new Ping(42);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(original.Serialize().AsMemory());
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void CreateLobby_Public_Roundtrip()
        {
            var original = new CreateLobby(123456789UL, LobbyVisibility.Public, 8);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(original.Serialize().AsMemory());
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void CreateLobby_Private_Roundtrip()
        {
            var original = new CreateLobby(987654321UL, LobbyVisibility.Private, 2);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(original.Serialize().AsMemory());
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void CreateLobby_FriendsOnly_Roundtrip()
        {
            var original = new CreateLobby(111222333UL, LobbyVisibility.FriendsOnly, 4);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(original.Serialize().AsMemory());
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void JoinLobby_Roundtrip()
        {
            var original = new JoinLobby(LobbyId: 555666777UL, UserId: 111222333UL);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(original.Serialize().AsMemory());
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void LeaveLobby_Roundtrip()
        {
            var original = new LeaveLobby(LobbyId: 999000111UL, UserId: 444555666UL);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(original.Serialize().AsMemory());
            Assert.AreEqual(original, deserialized);
        }
    }
}
