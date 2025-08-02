using Hints;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Extensions;
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

        if (FpcNoclip.IsPermitted(ev.Player.ReferenceHub))
            return;

        if (!ScpProximityChatPlugin.Instance.Config.AllowedRoles.Contains(ev.Player.Role))
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

    public override void OnPlayerSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
    {

        if (ev.Message.Channel != VoiceChatChannel.ScpChat)
            return;

        if (!ReferenceHub.TryGetHubNetID(ev.Player.NetworkId, out ReferenceHub player))
            return;

        if (!ScpProximityChatModule.Config.AllowedRoles.Contains(player.roleManager.CurrentRole.RoleTypeId) || (ScpProximityChatModule.Config.ToggleChat && !ToggledPlayers.Contains(player)))
            return;

        SendProximityMessage(ev.Message);

        return;
    }

    private static void SendProximityMessage(VoiceMessage msg)
    {
        CL.Debug("sending prox message");
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