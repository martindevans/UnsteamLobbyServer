using System.Collections.Concurrent;
using System.Net.WebSockets;
using HandySerialization.Wrappers;
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
        _manager.LobbyChatUpdate += HandleChatUpdateEvent;
        _manager.LobbyDataUpdate += HandleDataUpdateEvent;
    }

    #region event handlers
    private async ValueTask HandleChatUpdateEvent(LobbyManager.LobbyChatUpdateEvent @event)
    {
        await Broadcast(
            new LobbyChatUpdate(
                @event.LobbyId,
                @event.ChangedUserId,
                @event.ChangingUserId,
                @event.State
            )
        );
    }

    private async ValueTask HandleDataUpdateEvent(LobbyManager.LobbyDataUpdateEvent @event)
    {
        await Broadcast(
            new LobbyDataUpdate(
                @event.LobbyId,
                @event.MemberId,
                @event.Success,
                @event.LobbyData,
                @event.LobbyMemberData,
                @event.MemberCount,
                @event.MemberLimit
            )
        );
    }

    private async ValueTask Broadcast<TMessage>(TMessage message)
        where TMessage : BaseWebsocketMessageToClient
    {
        var ms = new MemoryStream();
        var writer = new StreamByteWriter(ms);
        message.Serialize(ref writer);
        var bytes = ms.ToArray();

        foreach (var connection in _connections.Keys)
        {
            await connection.SendAsync(
                bytes,
                WebSocketMessageType.Binary,
                WebSocketMessageFlags.EndOfMessage,
                CancellationToken.None
            );
        }
    }
    #endregion

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
            // Read from socket into this buffer
            var readBuffer = new byte[1024];

            // Copy bytes into this buffer until a complete message
            var messageBuffer = new byte[1024];

            while (websocket.State == WebSocketState.Open)
            {
                // Accumulate the full message into messageBuffer
                var messageBufferIndex = 0;
                WebSocketReceiveResult result;
                do
                {
                    // Receive some bytes
                    result = await websocket.ReceiveAsync(new ArraySegment<byte>(readBuffer), CancellationToken.None);

                    // Handle the closing event
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        return;
                    }

                    // Grow message buffer if necessary
                    if (messageBufferIndex + result.Count >= messageBuffer.Length)
                        Array.Resize(ref messageBuffer, Math.Max(messageBuffer.Length * 2, messageBuffer.Length + result.Count));

                    // Copy into message buffer
                    readBuffer.AsSpan(0, result.Count).CopyTo(messageBuffer.AsSpan(messageBufferIndex));
                    messageBufferIndex += result.Count;
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    // Get the bytes of the message we received
                    var bytes = messageBuffer.AsMemory(0, messageBufferIndex);
                    var reader = new MemoryByteReader(bytes);

                    // Deserialize message
                    var message = BaseWebsocketMessageToServer.Deserialize(ref reader);
                    if (message != null)
                        await HandleMessage(websocket, message);
                }
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogWarning("Websocket exception: {ex}", ex.WebSocketErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Exception: {ex}", ex);
        }
        finally
        {
            _connections.TryRemove(websocket, out _);
        }
    }

    public async Task<string> List(HttpContext ctx)
    {        
        return "Hello";
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
                var id = await _manager.Create(cl.Owner, cl.Visibility, cl.MaxMembers);
                await Reply(new LobbyCreated(id));
                await Reply(new LobbyDataUpdate(id, id, true, [], [], 1, cl.MaxMembers));
                break;
            }

            case JoinLobby jl:
            {
                var ok = await _manager.Join(jl.LobbyId, jl.UserId);
                
                await Reply(new LobbyEnter(jl.LobbyId, ok, _manager.GetLobbyData(jl.LobbyId), _manager.GetLobbyMemberData(jl.LobbyId)));
                if (ok)
                    await Broadcast(new LobbyChatUpdate(jl.LobbyId, jl.UserId, jl.UserId, ChatMemberStateChange.Entered));
                
                break;
            }

            case LeaveLobby ll:
            {
                await _manager.Leave(ll.LobbyId, ll.UserId);
                break;
            }

            case SetLobbyMemberLimit slml:
            {
                _manager.SetLobbyMemberLimit(slml.LobbyId, slml.Sender, slml.MaxMembers);
                break;
            }

            case SetLobbyVisibility slv:
            {
                _manager.SetLobbyVisibility(slv.LobbyId, slv.Sender, slv.Visibility);
                break;
            }

            case DeleteLobbyData dld:
            {
                await _manager.DeleteLobbyData(dld.LobbyId, dld.Sender, dld.Key);
                break;
            }

            case SetLobbyData sld:
            {
                await _manager.SetLobbyData(sld.LobbyId, sld.Sender, sld.Key, sld.Value);
                break;
            }

            case SetLobbyMemberData slmd:
            {
                await _manager.SetLobbyMemberData(slmd.LobbyId, slmd.Sender, slmd.Key, slmd.Value);
                break;
            }

            case SetLobbyOwner slo:
            {
                await _manager.SetLobbyOwner(slo.LobbyId, slo.Sender, slo.NewOwner);
                break;
            }

            case SendLobbyChat slc:
            {
                await Broadcast(new LobbyChatMessage(slc.LobbyId, slc.Sender, slc.Message));
                break;
            }
        }

        async ValueTask Reply<T>(T message)
            where T : BaseWebsocketMessageToClient
        {
            var ms = new MemoryStream();
            var writer = new StreamByteWriter(ms);
            message.Serialize(ref writer);
            var bytes = ms.ToArray();

            await socket.SendAsync(
                bytes,
                WebSocketMessageType.Binary,
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