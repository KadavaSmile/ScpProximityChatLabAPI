using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using ScpProximityChat.Features;

namespace ScpProximityChat;
public class ScpProximityChatPlugin : Plugin<Config>
{
    public static ScpProximityChatPlugin Instance;

    public override string Description => "Allows players to talk as SCP's!";
    public override string Name => "ScpProximityChat - LABAPI Port";
    public override string Author => "KadavaSmile";
    public override Version Version => new(0, 1);
    public override Version RequiredApiVersion => LabApi.Features.LabApiProperties.CurrentVersion;
    public override LoadPriority Priority => LoadPriority.Lowest;
    public ScpProximityChatHandler Handlers => new();

    public override void Enable()
    {
        Instance = this;
        CustomHandlersManager.RegisterEventsHandler(Handlers);
    }
    public override void Disable()
    {
        Instance = null;
        CustomHandlersManager.UnregisterEventsHandler(Handlers);
    }

}
