using UnsteamLobbyServer.Protocol;

namespace UnsteamLobbyServer.Lobbies;

public class LobbyManager
{
    private readonly Lock _lock = new();
    
    private readonly Random _random = new();
    private readonly Dictionary<ulong, Lobby> _lobbies = [ ];

    public event Func<LobbyDataUpdateEvent, ValueTask>? LobbyDataUpdate;
    public event Func<LobbyChatUpdateEvent, ValueTask>? LobbyChatUpdate;
    
    /// <summary>
    /// Create a new lobby and return the ID
    /// </summary>
    /// <returns></returns>
    public async Task<ulong> Create(ulong owner, LobbyVisibility visiblity, int maxMembers)
    {
        // Global lock on all lobby management operations
        ulong lobbyId;
        using (_lock.EnterScope())
        {

            // Pick an ID
            lobbyId = Enumerable
                .InfiniteSequence(0, 0)
                .Select(_ => unchecked((ulong)_random.NextInt64()))
                .First(x => !_lobbies.ContainsKey(x));

            // Create lobby
            var lobby = new Lobby(lobbyId, owner);
            lobby.SetVisibility(visiblity);
            lobby.SetMaxMembers(maxMembers);
            _lobbies[lobbyId] = lobby;
        }

        // Raise events
        await (LobbyDataUpdate?.Invoke(new LobbyDataUpdateEvent(lobbyId, lobbyId, true)) ?? ValueTask.CompletedTask);

        return lobbyId;
    }

    public async ValueTask<bool> Join(ulong lobbyId, ulong userId)
    {
        // Global lock on all lobby management operations
        using (_lock.EnterScope())
        {
            // Find lobby
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
                return false;

            // Join the lobby
            lobby.Join(userId);
        }

        // Send events
        await (LobbyChatUpdate?.Invoke(new LobbyChatUpdateEvent(
            lobbyId,
            userId,
            userId,
            ChatMemberStateChange.Entered
        )) ?? ValueTask.CompletedTask);

        return true;
    }

    public async ValueTask<bool> Leave(ulong lobbyId, ulong userId)
    {
        // Global lock on all lobby management operations
        bool removed;
        using (_lock.EnterScope())
        {
            // Find lobby
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
                return false;

            // Leave
            removed = lobby.Leave(userId);
        }

        if (removed)
        {
            await (LobbyChatUpdate?.Invoke(new LobbyChatUpdateEvent(
                lobbyId,
                userId,
                userId,
                ChatMemberStateChange.Left
            )) ?? ValueTask.CompletedTask);
        }

        return removed;
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
        
        return true;
    }
    
    public async ValueTask<bool> DeleteLobbyData(ulong lobbyId, ulong userId, string key)
    {
        // Global lock on all lobby management operations
        using (_lock.EnterScope())
        {

            // Try to get the lobby
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
                return false;

            // Check permission
            if (lobby.Owner != userId)
                return false;

            // Do the work
            lobby.DeleteLobbyData(key);
        }

        // Raise events
        await (LobbyDataUpdate?.Invoke(new LobbyDataUpdateEvent(lobbyId, lobbyId, true)) ?? ValueTask.CompletedTask);
        
        return true;
    }

    public async ValueTask<bool> SetLobbyData(ulong lobbyId, ulong userId, string key, string value)
    {
        // Global lock on all lobby management operations
        using (_lock.EnterScope())
        {
            // Try to get the lobby
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
                return false;

            // Check permission
            if (lobby.Owner != userId)
                return false;

            // Do the work
            lobby.SetLobbyData(key, value);
        }

        // Raise events
        await (LobbyDataUpdate?.Invoke(new LobbyDataUpdateEvent(lobbyId, lobbyId, true)) ?? ValueTask.CompletedTask);

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
    
    public async ValueTask<bool> SetLobbyMemberData(ulong lobbyId, ulong userId, string key, string value)
    {
        // Global lock on all lobby management operations
        using (_lock.EnterScope())
        {

            // Try to get the lobby
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
                return false;

            // Do the work
            lobby.SetLobbyMemberData(userId, key, value);
        }

        // Raise events
        await (LobbyDataUpdate?.Invoke(new LobbyDataUpdateEvent(lobbyId, userId, true)) ?? ValueTask.CompletedTask);

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

    public async ValueTask<bool> SetLobbyOwner(ulong lobbyId, ulong userId, ulong newOwnerId)
    {
        // Global lock on all lobby management operations
        using (_lock.EnterScope())
        {
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
        }

        // Raise events
        await (LobbyDataUpdate?.Invoke(new LobbyDataUpdateEvent(lobbyId, userId, true)) ?? ValueTask.CompletedTask);

        return true;
    }

    #region events
    public record LobbyDataUpdateEvent(ulong LobbyId, ulong MemberId, bool Success);
    
    /// <summary>
    /// Update indicating a change to lobby chat participation
    /// </summary>
    /// <param name="LobbyId">ID of lobby</param>
    /// <param name="ChangedUserId">ID of user being changed</param>
    /// <param name="ChangingUserId">ID of user making change, may be different fro Changed if e.g. banning or kicking</param>
    /// <param name="state">The state being changed</param>
    public record LobbyChatUpdateEvent(ulong LobbyId, ulong ChangedUserId, ulong ChangingUserId, ChatMemberStateChange State);
    #endregion
}