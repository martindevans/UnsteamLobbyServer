using UnsteamLobbyServer.Protocol;

namespace UnsteamLobbyServer.Lobbies;

public class Lobby
{
    public ulong Id { get; }
    public ulong Owner { get; private set; }

    public IReadOnlyCollection<ulong> Members => _lobbyMemberData.Keys;

    private readonly Dictionary<string, string> _lobbyData = new();
    private readonly Dictionary<ulong, Dictionary<string, string>> _lobbyMemberData = new();
    
    private LobbyVisibility _visibility;
    private int _maxMembers;

    public Lobby(ulong id, ulong owner)
    {
        Id = id;
        Owner = owner;
    }

    public void DeleteLobbyData(string key)
    {
        _lobbyData.Remove(key);
    }

    public void SetLobbyData(string key, string value)
    {
        _lobbyData[key] = value;
    }

    public void SetLobbyMemberData(ulong userId, string key, string value)
    {
        if (!_lobbyMemberData.TryGetValue(userId, out var userData))
        {
            userData = new Dictionary<string, string>();
            _lobbyMemberData[userId] = userData;
        }
        
        userData[key] = value;
    }

    public void SetVisibility(LobbyVisibility visiblity)
    {
        _visibility = visiblity;
    }

    public void SetMaxMembers(int maxMembers)
    {
        _maxMembers = maxMembers;
    }

    public void SetLobbyOwner(ulong owner)
    {
        Owner = owner;
    }

    public IEnumerable<KeyValuePair<string, string>> GetLobbyData()
    {
        return _lobbyData.ToArray();
    }

    public IEnumerable<KeyValuePair<(ulong, string), string>> GetLobbyMemberData()
    {
        return from kvpo in _lobbyMemberData
               let user = kvpo.Key
               from kvpi in kvpo.Value
               select new KeyValuePair<(ulong, string), string>((user, kvpi.Key), kvpi.Value);
    }

    public void Join(ulong userId)
    {
        _lobbyMemberData.Add(userId, new Dictionary<string, string>());
    }

    public bool Leave(ulong userId)
    {
        return _lobbyMemberData.Remove(userId);
    }
}