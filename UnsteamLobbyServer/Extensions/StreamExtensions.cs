namespace UnsteamLobbyServer.Protocol.Extensions;

public static class StreamExtensions
{
    public static async Task<string> ReadStringToEndAsync(this Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}