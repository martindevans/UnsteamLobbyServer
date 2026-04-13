using HandySerialization.Wrappers;
using Spectre.Console;
using System.Net.WebSockets;
using UnsteamLobbyServer.Protocol;

namespace UnsteamLobbyClient;

public static class SocketHelpers
{
    #region helpers
    public static async Task<ClientWebSocket?> Connect(CancellationToken cancellation)
    {
        // Establish websocket connection to server
        var socket = new ClientWebSocket();

        return await AnsiConsole.Status().StartAsync<ClientWebSocket?>("Connecting...", async ctx =>
        {
            try
            {
                await socket.ConnectAsync(new Uri("ws://localhost:5030/connect"), cancellation);

                // Send ping
                var pingId = new Random().Next();
                var ping = new Ping(pingId);
                await Send(socket, ping, cancellation);

                // Receive pong
                var buffer = new byte[1024];
                var result = await socket.ReceiveAsync(buffer, cancellation);
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    var reader = new MemoryByteReader(buffer.AsMemory(0, result.Count));
                    var response = BaseWebsocketMessageToClient.Deserialize(ref reader);
                    if (response is Pong pong && pong.ID == pingId)
                    {
                        ctx.Status("[green]Connected.[/]");
                        await Task.Delay(500, cancellation);
                        return socket;
                    }

                    ctx.Status("[red]Failed to connect.[/]");
                    await Task.Delay(1500, cancellation);
                }
            }
            catch (Exception ex)
            {
                ctx.Status($"[red]Exception connecting[/]: {ex.Message}");
                await Task.Delay(1500, cancellation);
            }

            return null;
        });
    }

    public static async Task Send<TMessage>(ClientWebSocket socket, TMessage message, CancellationToken cancellation)
        where TMessage : BaseWebsocketMessageToServer
    {
        var ms = new MemoryStream();
        var writer = new StreamByteWriter(ms);
        message.Serialize(ref writer);

        await socket.SendAsync(ms.ToArray(), WebSocketMessageType.Binary, true, cancellation);
    }
    #endregion
}