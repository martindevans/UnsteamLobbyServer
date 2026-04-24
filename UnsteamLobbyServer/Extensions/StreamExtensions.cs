namespace UnsteamLobbyServer.Extensions;

public static class StreamExtensions
{
    public static async Task<string> ReadStringToEndAsync(this Stream stream, bool leaveOpen = false)
    {
        using var reader = new StreamReader(stream, leaveOpen: leaveOpen);
        return await reader.ReadToEndAsync();
    }
}