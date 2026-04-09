using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text.Json;
using UnsteamLobbyServer.Protocol;
using Ping = UnsteamLobbyServer.Protocol.Ping;

namespace UnsteamLobbyServer.Lobbies;

public partial class LobbyServer
{
    private readonly ILogger<LobbyServer> _logger;
        
    private readonly LobbyManager _manager;
    private readonly ConcurrentDictionary<WebSocket, byte> _connections = new();

    public LobbyServer(ILogger<LobbyServer> logger)
    {
        _logger = logger;
        _manager = new LobbyManager();
    }
        
    public async Task Connect(HttpContext ctx)
    {
        // Sanity check that this is a websocket request
        if (!ctx.WebSockets.IsWebSocketRequest)
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
            
        // Accept the connection
        using var websocket = await ctx.WebSockets.AcceptWebSocketAsync();
        _connections.TryAdd(websocket, 0);
        LogAcceptedConnection(ctx.Connection);

        try
        {
            var buffer = new byte[1024];

            while (websocket.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;

                // Accumulate the full message into memorystream
                do
                {
                    result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        return;
                    }

                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                // Prepare the data for processing
                ms.Seek(0, SeekOrigin.Begin);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = BaseWebsocketMessageToServer.Deserialize(ms);
                    if (message != null)
                        await HandleMessage(websocket, message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Websocket exception: {ex}", ex);
        }
        finally
        {
            _connections.TryRemove(websocket, out _);
        }
    }

    private async Task HandleMessage(WebSocket socket, BaseWebsocketMessageToServer message, CancellationToken cancellation = default)
    {
        switch (message)
        {
            case Ping p:
            {
                await Reply(new Pong(p.ID));
                break;
            }

            case CreateLobby cl:
            {
                var id = _manager.Create(cl.Owner, cl.Visibility, cl.MaxMembers);
                await Reply(new LobbyCreated(id));
                break;
            }

            case JoinLobby jl:
            {
                var ok = _manager.Join(jl.LobbyId, jl.UserId);
                await Reply(new LobbyEnter(jl.LobbyId, ok));
                break;
            }

            case LeaveLobby ll:
            {
                _manager.Leave(ll.LobbyId, ll.UserId);
                break;
            }
        }

        async ValueTask Reply<T>(T message)
            where T : BaseWebsocketMessageToClient
        {
            await socket.SendAsync(
                message.Serialize(),
                WebSocketMessageType.Text,
                WebSocketMessageFlags.EndOfMessage,
                cancellation
            );
        }
    }

    #region logging
    [LoggerMessage(Level = LogLevel.Information, Message = "Accepted WebSocket connection: {conn}")]
    private partial void LogAcceptedConnection(ConnectionInfo conn);
    #endregion
}