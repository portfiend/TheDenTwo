using Content.Shared._DEN.Overlays.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._DEN.Overlays;

public sealed partial class NightVisionOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "NightVision";

    [Dependency] private IPrototypeManager _prototypeManager = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _nightVisionShader;

    public NightVisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _nightVisionShader = _prototypeManager.Index(Shader).InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        _nightVisionShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        var handle = args.WorldHandle;
        handle.UseShader(_nightVisionShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    /// <summary>
    ///     Sets the tint color of this overlay.
    /// </summary>
    /// <param name="tint">The tint color.</param>
    public void SetColorTint(Color tint)
    {
        _nightVisionShader.SetParameter("tint_color", tint);
    }
}
