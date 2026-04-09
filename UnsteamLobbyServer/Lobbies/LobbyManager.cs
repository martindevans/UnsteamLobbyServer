using UnsteamLobbyServer.Protocol;

namespace UnsteamLobbyServer.Lobbies;

public class LobbyManager
{
    private readonly Lock _lock = new();
    
    private readonly Random _random = new();
    private readonly Dictionary<ulong, Lobby> _lobbies = new();

    //todo: hook up events and send messages to clients
    public event Action<LobbyCreatedEventData>? LobbyCreated;
    public event Action<LobbyEnterEvent>? LobbyEnter;
    public event Action<LobbyDataUpdateEvent>? LobbyDataUpdate;
    public event Action<LobbyChatUpdateEvent>? LobbyChatUpdate;
    
    /// <summary>
    /// Create a new lobby and return the ID
    /// </summary>
    /// <returns></returns>
    public ulong Create(ulong owner, LobbyVisibility visiblity, int maxMembers)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Pick an ID
        var id = Enumerable
            .InfiniteSequence(0, 0)
            .Select(_ => unchecked((ulong)_random.NextInt64()))
            .First(x => !_lobbies.ContainsKey(x));
        
        // Create lobby
        var lobby = new Lobby(id, owner);
        lobby.SetVisibility(visiblity);
        lobby.SetMaxMembers(maxMembers);
        _lobbies[id] = lobby;
        
        // Raise events
        LobbyCreated?.Invoke(new LobbyCreatedEventData(id, owner));
        LobbyEnter?.Invoke(new LobbyEnterEvent(id, owner));
        LobbyDataUpdate?.Invoke(new LobbyDataUpdateEvent(id, id, true));

        return id;
    }

    public bool Join(ulong lobbyId, ulong userId)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Find lobby
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            return false;

        // Join the lobby
        lobby.Join(userId);

        LobbyChatUpdate?.Invoke(new LobbyChatUpdateEvent(
            lobbyId,
            userId,
            userId,
            ChatMemberStateChange.Entered
        ));

        return true;
    }

    public void Leave(ulong lobbyId, ulong userId)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Find lobby
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            return;
        
        // Leave
        var removed = lobby.Leave(userId);
        if (removed)
        {
            LobbyChatUpdate?.Invoke(new LobbyChatUpdateEvent(
                lobbyId,
                userId,
                userId,
                ChatMemberStateChange.Left
            ));
        }
    }

    public bool SetLobbyMemberLimit(ulong lobbyId, ulong userId, int memberLimit)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Try to get the lobby
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            return false;

        // Check permission
        if (lobby.Owner != userId)
            return false;
        
        // Do the work
        lobby.SetMaxMembers(memberLimit);
        
        //todo: no events?

        return true;
    }
    
    public bool SetLobbyVisibility(ulong lobbyId, ulong userId, LobbyVisibility visibility)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Try to get the lobby
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            return false;

        // Check permission
        if (lobby.Owner != userId)
            return false;
        
        // Do the work
        lobby.SetVisibility(visibility);
        
        //todo: no events?

        return true;
    }
    
    public bool DeleteLobbyData(ulong lobbyId, ulong userId, string key)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Try to get the lobby
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            return false;

        // Check permission
        if (lobby.Owner != userId)
            return false;

        // Do the work
        lobby.DeleteLobbyData(key);
        
        // Raise events
        LobbyDataUpdate?.Invoke(new LobbyDataUpdateEvent(lobbyId, lobbyId, true));
        
        return true;
    }

    public bool SetLobbyData(ulong lobbyId, ulong userId, string key, string value)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Try to get the lobby
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            return false;

        // Check permission
        if (lobby.Owner != userId)
            return false;

        // Do the work
        lobby.SetLobbyData(key, value);

        // Raise events
        LobbyDataUpdate?.Invoke(new LobbyDataUpdateEvent(lobbyId, lobbyId, true));

        return true;
    }

    public IReadOnlyList<KeyValuePair<string, string>> GetLobbyData(ulong lobbyId)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Try to get the lobby
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            return [];

        return lobby.GetLobbyData().ToArray();
    }
    
    public bool SetLobbyMemberData(ulong lobbyId, ulong userId, string key, string value)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Try to get the lobby
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            return false;

        // Do the work
        lobby.SetLobbyMemberData(userId, key, value);

        // Raise events
        LobbyDataUpdate?.Invoke(new LobbyDataUpdateEvent(lobbyId, userId, true));

        return true;
    }

    public IReadOnlyList<KeyValuePair<(ulong, string), string>> GetLobbyMemberData(ulong lobbyId)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Try to get the lobby
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            return [];

        return lobby.GetLobbyMemberData().ToArray();
    }

    public bool SetLobbyOwner(ulong lobbyId, ulong userId, ulong newOwnerId)
    {
        // Global lock on all lobby management operations
        using var scope = _lock.EnterScope();

        // Try to get the lobby
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            return false;

        // Check permission
        if (lobby.Owner != userId)
            return false;

        if (!lobby.Members.Contains(newOwnerId))
            return false;
        
        // Do the work
        lobby.SetLobbyOwner(newOwnerId);

        // Raise events
        LobbyDataUpdate?.Invoke(new LobbyDataUpdateEvent(lobbyId, lobbyId, true));

        return true;
    }

    #region events
    public record LobbyCreatedEventData(ulong Lobby, ulong Owner);
    public record LobbyEnterEvent(ulong Lobby, ulong Member);
    public record LobbyDataUpdateEvent(ulong LobbyId, ulong MemberId, bool Success);
    
    /// <summary>
    /// Update indicating a change to lobby chat participation
    /// </summary>
    /// <param name="LobbyId">ID of lobby</param>
    /// <param name="ChangedUserId">ID of user being changed</param>
    /// <param name="ChangingUserId">ID of user making change, may be different fro Changed if e.g. banning or kicking</param>
    /// <param name="state">The state being changed</param>
    public record LobbyChatUpdateEvent(ulong LobbyId, ulong ChangedUserId, ulong ChangingUserId, ChatMemberStateChange state);
    #endregion
}