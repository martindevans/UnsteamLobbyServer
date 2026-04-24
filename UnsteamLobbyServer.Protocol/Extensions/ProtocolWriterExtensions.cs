using HandySerialization;

namespace UnsteamLobbyServer.Protocol.Extensions;

public static class ProtocolWriterExtensions
{
    public static void Write<TWriter>(ref this TWriter writer, BaseWebsocketMessageToServer packet)
        where TWriter : struct, IByteWriter
    {
        packet.Serialize(ref writer);
    }

    public static void Write<TWriter>(ref this TWriter writer, BaseWebsocketMessageToClient packet)
        where TWriter : struct, IByteWriter
    {
        packet.Serialize(ref writer);
    }
}