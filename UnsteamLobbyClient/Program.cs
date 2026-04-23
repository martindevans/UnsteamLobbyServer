using System.Net.WebSockets;
using System.Threading.Channels;
using HandySerialization.Wrappers;
using Spectre.Console;
using UnsteamLobbyClient;
using UnsteamLobbyServer.Protocol;

// Display figlet banner
var banner = new FigletText("Unsteam.Lobby")
{
    Color = Color.Green,
    Justification = Justify.Center
};
AnsiConsole.Write(banner);

// Random client ID
var clientId = unchecked((ulong)new Random().NextInt64());
AnsiConsole.MarkupLineInterpolated($"Client ID: {clientId}");

// Connect to server
var socket = await WebsocketHelpers.Connect(default);
if (socket == null)
    return;

// Set up handlers for events
var events = Channel.CreateUnbounded<BaseQueueEvent>();

// Drain event queue
_ = Task.Run(EventProcessingLoop);

// Received packets, push to event queue
_ = Task.Run(SocketReceiveLoop);

// Read user input, push to event queue
while (true)
{
    var prompt = new SelectionPrompt<string>()
                .Title("Select an [green]action[/]:")
                .AddChoices("Host", "MassHost", "Join", "Leave", "Chat", "SetData", "SetUserData", "DisplayData", "Exit")
                .HighlightStyle(Style.Plain);
    var choice = await AnsiConsole.PromptAsync(prompt);
    
    switch (choice)
    {
        case "Exit":
            return;

        case "Host":
        {
            await events.Writer.WriteAsync(new UserInputHost());
            break;
        }

        case "MassHost":
        {
            var count = await AnsiConsole.PromptAsync(new TextPrompt<int>("How many lobbies?"));
            await events.Writer.WriteAsync(new UserInputMassHost(count));
            break;
        }

        case "Join":
        {
            var joinId = await AnsiConsole.PromptAsync(new TextPrompt<ulong>("Lobby ID: "));
            AnsiConsole.MarkupLineInterpolated($"Joining...");
            await events.Writer.WriteAsync(new UserInputJoin(joinId));
            break;
        }

        case "Leave":
        {
            await events.Writer.WriteAsync(new UserInputLeave());
            AnsiConsole.MarkupLineInterpolated($"Leaving lobby...");
            break;
        }

        case "Chat":
        {
            var message = await AnsiConsole.PromptAsync(new TextPrompt<string>("Message: "));
            await events.Writer.WriteAsync(new UserInputChat(message));
            break;
        }

        case "SetData":
        {
            var key = await AnsiConsole.PromptAsync(new TextPrompt<string>("Key: "));
            var val = await AnsiConsole.PromptAsync(new TextPrompt<string>("Val: "));
            await events.Writer.WriteAsync(new UserInputSetLobbyData(key, val));
            break;
        }

        case "SetUserData":
        {
            var key = await AnsiConsole.PromptAsync(new TextPrompt<string>("Key: "));
            var val = await AnsiConsole.PromptAsync(new TextPrompt<string>("Val: "));
            await events.Writer.WriteAsync(new UserInputSetMemberData(key, val));
            break;
        }

        case "DisplayData":
        {
            await events.Writer.WriteAsync(new UserInputPrintData());
            break;
        }
    }
}

async Task SocketReceiveLoop()
{
    var readBuffer = new byte[4096];
    var messageBuffer = new byte[1024];

    while (socket.State == WebSocketState.Open)
    {
        try
        {
            // Accumulate the full message into messageBuffer
            var messageBufferIndex = 0;
            WebSocketReceiveResult result;
            do
            {
                // Receive some bytes
                result = await socket.ReceiveAsync(new ArraySegment<byte>(readBuffer), CancellationToken.None);

                // Handle the closing event
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    AnsiConsole.MarkupLineInterpolated($"[red]Websocket closed![/]");
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

            // Copy complete message and enqueue processing event
            await events.Writer.WriteAsync(new SocketData(messageBuffer.AsMemory(0, messageBufferIndex).ToArray()));
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error receiving message: {Markup.Escape(ex.Message)}[/]");
            break;
        }
    }
}

async Task EventProcessingLoop()
{
    try
    {
        LobbyData? lobbyData = null;
        while (true)
            await foreach (var item in events.Reader.ReadAllAsync())
                lobbyData = await item.Handle(socket, clientId, lobbyData);
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLineInterpolated($"[red]Event Processing Loop Exception! {Markup.Escape(ex.Message)}[/]");
    }

    AnsiConsole.MarkupLineInterpolated($"[red]Event Processing Loop Exit![/]");
}

internal readonly struct LobbyData
{
    public readonly ulong ID;
    public readonly IReadOnlyDictionary<string, string> Data;
    public readonly IReadOnlyDictionary<(ulong, string), string> UserData;

    public LobbyData(ulong id, Dictionary<string, string> data, Dictionary<(ulong, string), string> userData)
    {
        ID = id;
        Data = data;
        UserData = userData;
    }
}

internal abstract record BaseQueueEvent
{
    public abstract Task<LobbyData?> Handle(ClientWebSocket socket, ulong clientId, LobbyData? lobbyData);
}

internal record UserInputHost
    : BaseQueueEvent
{
    public override async Task<LobbyData?> Handle(ClientWebSocket socket, ulong clientId, LobbyData? lobbyData)
    {
        await WebsocketHelpers.Send(
            socket,
            new CreateLobby(clientId, LobbyVisibility.Public, 8),
            default
        );
        
        return lobbyData;
    }
}

internal record UserInputMassHost(int count)
    : BaseQueueEvent
{
    public override async Task<LobbyData?> Handle(ClientWebSocket socket, ulong clientId, LobbyData? lobbyData)
    {
        for (var i = 0; i < count; i++)
        {
            await WebsocketHelpers.Send(
                socket,
                new CreateLobby(clientId, LobbyVisibility.Public, 8),
                default
            );
        }
        
        return lobbyData;
    }
}

internal record UserInputJoin(ulong LobbyId)
    : BaseQueueEvent
{
    public override async Task<LobbyData?> Handle(ClientWebSocket socket, ulong clientId, LobbyData? lobbyData)
    {
        await WebsocketHelpers.Send(
            socket,
            new JoinLobby(LobbyId, clientId),
            default
        );

        return lobbyData;
    }
}

internal record UserInputLeave
    : BaseQueueEvent
{
    public override async Task<LobbyData?> Handle(ClientWebSocket socket, ulong clientId, LobbyData? lobbyData)
    {
        if (!lobbyData.HasValue)
            return null;

        await WebsocketHelpers.Send(
            socket,
            new LeaveLobby(lobbyData.Value.ID, clientId),
            default
        );

        return null;
    }
}

internal record UserInputChat(string Message)
    : BaseQueueEvent
{
    public override async Task<LobbyData?> Handle(ClientWebSocket socket, ulong clientId, LobbyData? lobbyData)
    {
        if (!lobbyData.HasValue)
            return null;

        await WebsocketHelpers.Send(
            socket,
            new SendLobbyChat(lobbyData.Value.ID, clientId, Message),
            default
        );

        return lobbyData;
    }
}

internal record UserInputSetLobbyData(string Key, string Value)
    : BaseQueueEvent
{
    public override async Task<LobbyData?> Handle(ClientWebSocket socket, ulong clientId, LobbyData? lobbyData)
    {
        if (!lobbyData.HasValue)
            return null;

        await WebsocketHelpers.Send(
            socket,
            new SetLobbyData(lobbyData.Value.ID, clientId, Key, Value),
            default
        );

        return lobbyData;
    }
}

internal record UserInputSetMemberData(string Key, string Value)
    : BaseQueueEvent
{
    public override async Task<LobbyData?> Handle(ClientWebSocket socket, ulong clientId, LobbyData? lobbyData)
    {
        if (!lobbyData.HasValue)
            return null;

        await WebsocketHelpers.Send(
            socket,
            new SetLobbyMemberData(lobbyData.Value.ID, clientId, Key, Value),
            default
        );

        return lobbyData;
    }
}

internal record UserInputPrintData
    : BaseQueueEvent
{
    public override async Task<LobbyData?> Handle(ClientWebSocket socket, ulong clientId, LobbyData? lobbyData)
    {
        if (!lobbyData.HasValue)
        {
            AnsiConsole.MarkupLineInterpolated($"Cannot print lobby data - not in a lobby");
            return null;
        }

        var dataTable = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .Title("[yellow bold]Lobby Data[/]")
            .AddColumn("Key")
            .AddColumn("Value");

        foreach (var (key, value) in lobbyData.Value.Data)
            dataTable.AddRow(key, value);

        AnsiConsole.Write(dataTable);

        var userDataTable = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .Title("[yellow bold]Lobby User Data[/]")
            .AddColumn("Key")
            .AddColumn("Value");

        foreach (var group in lobbyData.Value.UserData.GroupBy(a => a.Key.Item1))
        {
            userDataTable.AddRow($"User: {group.Key}");

            foreach (var (key, value) in group)
                userDataTable.AddRow(key.Item2, value);
        }

        AnsiConsole.Write(userDataTable);

        return lobbyData;
    }
}

internal record SocketData(byte[] Data)
    : BaseQueueEvent
{
    public override async Task<LobbyData?> Handle(ClientWebSocket socket, ulong clientId, LobbyData? lobbyData)
    {
        var reader = new MemoryByteReader(Data);
        var message = BaseWebsocketMessageToClient.Deserialize(ref reader);

        switch (message)
        {
            case LobbyCreated lc:
            {
                AnsiConsole.MarkupLineInterpolated($"[green]Lobby created with ID: {lc.LobbyId}[/]");
                return new LobbyData(
                    lc.LobbyId,
                    new(),
                    new()
                );
            }

            case LobbyEnter le:
            {
                if (le.Success)
                {
                    AnsiConsole.MarkupLineInterpolated($"[green]Entered lobby {le.LobbyId}[/]");
                    return new LobbyData(
                        le.LobbyId,
                        le.LobbyData.ToDictionary(),
                        le.LobbyMemberData.ToDictionary()
                    );
                }

                AnsiConsole.MarkupLineInterpolated($"[red]Failed to enter lobby {le.LobbyId}[/]");
                break;
            }

            case LobbyChatUpdate lcu:
            {
                if (!lobbyData.HasValue || lcu.LobbyId != lobbyData.Value.ID)
                    break;

                AnsiConsole.MarkupLineInterpolated($"[yellow]Chat update in {lcu.LobbyId}: User {lcu.UserChangedId} is now {lcu.State}[/]");
                break;
            }

            case LobbyDataUpdate ldu:
            {
                if (!lobbyData.HasValue || ldu.LobbyId != lobbyData.Value.ID)
                    break;

                AnsiConsole.MarkupLineInterpolated($"[blue]Data update in {ldu.LobbyId} for {ldu.UserId} (Success: {ldu.Success})[/]");
                return new LobbyData(
                    ldu.LobbyId,
                    ldu.LobbyData.ToDictionary(a => a.Key, a => a.Value),
                    ldu.LobbyMemberData.ToDictionary(a => a.Key, a => a.Value)
                );
            }

            case LobbyChatMessage lcm:
            {
                if (!lobbyData.HasValue || lcm.LobbyId != lobbyData.Value.ID)
                    break;
                
                AnsiConsole.MarkupLineInterpolated($"{lcm.UserId}: {Markup.Escape(lcm.Message)}");
                break;
            }
            
            case Pong p:
            {
                AnsiConsole.MarkupLineInterpolated($"[blue]Received Pong {p.ID}");
                break;
            }
        }

        return lobbyData;
    }
}