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
                ctx.Status("[green]Connected.[/]");
                return socket;
            }
            catch (Exception ex)
            {
                ctx.Status($"[red]Exception connecting[/]: {Markup.Escape(ex.Message)}");
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