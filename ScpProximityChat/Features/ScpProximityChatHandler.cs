using Hints;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Extensions;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Spectating;
using PlayerRoles.Voice;
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;

namespace ScpProximityChat.Features;

public class ScpProximityChatHandler : CustomEventsHandler
{
    public static readonly HashSet<ReferenceHub> ToggledPlayers = [];

    public override void OnPlayerTogglingNoclip(PlayerTogglingNoclipEventArgs ev)
    {
        base.OnPlayerTogglingNoclip(ev);

        if (FpcNoclip.IsPermitted(ev.Player.ReferenceHub))
            return;

        

        if (!ScpProximityChatModule.Config.AllowedRoles.Contains(ev.Player.Role.GetRoleBase().RoleTypeId))
            return;

        if (!ToggledPlayers.Add(ev.Player.ReferenceHub))
        {
            ToggledPlayers.Remove(ev.Player.ReferenceHub);
            ev.Player.SendHint(ScpProximityChatModule.Config.ProximityChatDisabledMessage, [new StringHintParameter(string.Empty)], null, 4);
            return;
        }

        ev.Player.SendHint(ScpProximityChatModule.Config.ProximityChatEnabledMessage, [new StringHintParameter(string.Empty)], null, 4);
        return;
    }

    public static bool OnPlayerTogglingNoClip(ReferenceHub player)
    {
        if (FpcNoclip.IsPermitted(player))
            return true;

        if (!ScpProximityChatModule.Config.AllowedRoles.Contains(player.roleManager.CurrentRole.RoleTypeId))
            return true;

        if (!ToggledPlayers.Add(player))
        {
            ToggledPlayers.Remove(player);
            player.hints.Show(new TextHint(ScpProximityChatModule.Config.ProximityChatDisabledMessage, [new StringHintParameter(string.Empty)], null, 4));
            return false;
        }

        player.hints.Show(new TextHint(ScpProximityChatModule.Config.ProximityChatEnabledMessage, [new StringHintParameter(string.Empty)], null, 4));
        return false;
    }

    public static bool OnPlayerUsingVoiceChat(NetworkConnection connection, VoiceMessage message)
    {
        if (message.Channel != VoiceChatChannel.ScpChat)
            return true;

        if (!ReferenceHub.TryGetHubNetID(connection.identity.netId, out ReferenceHub player))
            return true;

        if (!ScpProximityChatModule.Config.AllowedRoles.Contains(player.roleManager.CurrentRole.RoleTypeId) || (ScpProximityChatModule.Config.ToggleChat && !ToggledPlayers.Contains(player)))
            return true;

        SendProximityMessage(message);
        return !ScpProximityChatModule.Config.ToggleChat;
    }

    private static void SendProximityMessage(VoiceMessage msg)
    {
        foreach (ReferenceHub referenceHub in ReferenceHub.AllHubs)
        {
            if (referenceHub.roleManager.CurrentRole.RoleTypeId is RoleTypeId.Spectator && !msg.Speaker.IsSpectatedBy(referenceHub))
                continue;

            if (referenceHub.roleManager.CurrentRole is not IVoiceRole voiceRole2)
                continue;

            if (Vector3.Distance(msg.Speaker.transform.position, referenceHub.transform.position) >= ScpProximityChatModule.Config.MaxProximityDistance)
                continue;

            if (voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, VoiceChatChannel.Proximity) is VoiceChatChannel.None)
                continue;

            msg.Channel = VoiceChatChannel.Proximity;
            referenceHub.connectionToClient.Send(msg);
        }
    }
}