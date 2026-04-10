using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
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
            // Read from socket into this buffer
            var readBuffer = new byte[1024];

            // Copy bytes into this buffer until a complete message
            var messageBuffer = new byte[1024];
            
            // Convert bytes to chars in this buffer
            var charsBuffer = new char[1024];

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
                }
                while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Get the bytes of the message we received
                    var bytes = messageBuffer.AsSpan(0, messageBufferIndex);

                    // Grow chars buffer if neccesary
                    var charCount = Encoding.UTF8.GetCharCount(bytes);
                    if (charCount > charsBuffer.Length)
                        Array.Resize(ref charsBuffer, charCount);

                    // Convert bytes to chars
                    charCount = Encoding.UTF8.GetChars(bytes, charsBuffer);
                    
                    // Deserialize message
                    var message = BaseWebsocketMessageToServer.Deserialize(charsBuffer.AsMemory(0, charCount));
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

            case SetLobbyMemberLimit slml:
            {
                _manager.SetLobbyMemberLimit(slml.LobbyId, slml.Sender, slml.MaxMembers);
                break;
            }
        }

        async ValueTask Reply<T>(T message)
            where T : BaseWebsocketMessageToClient
        {
            await socket.SendAsync(
                Encoding.UTF8.GetBytes(message.Serialize()),
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