using HandySerialization.Wrappers;
using UnsteamLobbyServer.Protocol;

namespace UnsteamLobbyServer.Tests
{
    [TestClass]
    public class RoundtripMessageToServer
    {
        private static StreamByteReader SerializeToReader(BaseWebsocketMessageToServer packet)
        {
            var ms = new MemoryStream();
            var writer = new StreamByteWriter(ms);

            packet.Serialize(ref writer);

            ms.Position = 0;
            return new StreamByteReader(ms);
        }

        [TestMethod]
        public void Ping_Roundtrip()
        {
            var original = new Ping(42);

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void CreateLobby_Public_Roundtrip()
        {
            var original = new CreateLobby(123456789UL, LobbyVisibility.Public, 8);

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void CreateLobby_Private_Roundtrip()
        {
            var original = new CreateLobby(987654321UL, LobbyVisibility.Private, 2);

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void CreateLobby_FriendsOnly_Roundtrip()
        {
            var original = new CreateLobby(111222333UL, LobbyVisibility.FriendsOnly, 4);

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void JoinLobby_Roundtrip()
        {
            var original = new JoinLobby(LobbyId: 555666777UL, UserId: 111222333UL);

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void LeaveLobby_Roundtrip()
        {
            var original = new LeaveLobby(LobbyId: 999000111UL, UserId: 444555666UL);

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void SetLobbyVisibility_Roundtrip()
        {
            var original = new SetLobbyVisibility(LobbyId: 111222333UL, Sender: 444555666UL, Visibility: LobbyVisibility.Private);

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void DeleteLobbyData_Roundtrip()
        {
            var original = new DeleteLobbyData(LobbyId: 111222333UL, Sender: 444555666UL, Key: "someKey");

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void SetLobbyData_Roundtrip()
        {
            var original = new SetLobbyData(LobbyId: 111222333UL, Sender: 444555666UL, Key: "someKey", Value: "someValue");

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void SetLobbyMemberData_Roundtrip()
        {
            var original = new SetLobbyMemberData(LobbyId: 111222333UL, Sender: 444555666UL, Key: "someKey", Value: "someValue");

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void SetLobbyOwner_Roundtrip()
        {
            var original = new SetLobbyOwner(LobbyId: 111222333UL, Sender: 444555666UL, NewOwner: 777888999UL);

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);
            
            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void SetLobbyMemberLimit_Roundtrip()
        {
            var original = new SetLobbyMemberLimit(99, 88, 77);

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);

            Assert.AreEqual(original, deserialized);
        }

        [TestMethod]
        public void SendLobbyChat_Roundtrip()
        {
            var original = new SendLobbyChat(99, 88, "hi");

            var reader = SerializeToReader(original);
            var deserialized = BaseWebsocketMessageToServer.Deserialize(ref reader);

            Assert.AreEqual(original, deserialized);
        }
    }
}
