using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using PlayerRoles;
using ScpProximityChat.Features;

namespace ScpProximityChat;

public class ScpProximityChatModule : CustomEventsHandler
{
    public override void OnServerRoundStarted()
    {
        base.OnServerRoundStarted();
        ScpProximityChatHandler.ToggledPlayers.Clear();
    }
    public static Config Config { get; set; } = new Config();
    public override void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev)
    {
        base.OnPlayerChangingRole(ev);

        if (!Config.SendBroadcastOnRoleChange)
            return;

        if (!Config.AllowedRoles.Contains(ev.NewRole))
            return;

        ev.Player.SendBroadcast(Config.BroadcastMessage, Config.BroadcastDuration);
    }

}