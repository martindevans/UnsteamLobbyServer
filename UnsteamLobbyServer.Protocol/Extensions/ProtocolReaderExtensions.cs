using HandySerialization;

namespace UnsteamLobbyServer.Protocol.Extensions
{
    public static class ProtocolReaderExtensions
    {
        public static TPacket? ReadBaseWebsocketMessageToServer<TReader, TPacket>(this TReader reader)
            where TReader : struct, IByteReader
            where TPacket : BaseWebsocketMessageToServer
        {
            return (TPacket?)BaseWebsocketMessageToServer.Deserialize(ref reader);
        }

        public static TPacket? ReadBaseWebsocketMessageToClient<TReader, TPacket>(this TReader reader)
            where TReader : struct, IByteReader
            where TPacket : BaseWebsocketMessageToClient
        {
            return (TPacket?)BaseWebsocketMessageToClient.Deserialize(ref reader);
        }
    }
}
