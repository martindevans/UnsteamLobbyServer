namespace UnsteamLobbyServer.Protocol;

public enum LobbyVisibility
{
    /// <summary>
    /// Anyone can join
    /// </summary>
    Public,

    /// <summary>
    /// No one can join unless invited
    /// </summary>
    Private,

    /// <summary>
    /// Only friends can join
    /// </summary>
    FriendsOnly,
}