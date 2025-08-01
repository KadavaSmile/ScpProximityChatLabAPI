using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using ScpProximityChat.Features;

namespace ScpProximityChat;

public class ScpProximityChatModule : CustomEventsHandler
{
    public static Config Config { get; set; } = new Config();
    public override void OnServerRoundStarted()
    {
        base.OnServerRoundStarted();
        ScpProximityChatHandler.ToggledPlayers.Clear();
    }
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