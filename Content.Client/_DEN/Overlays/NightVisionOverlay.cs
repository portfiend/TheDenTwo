using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._DEN.Overlays;

public sealed partial class NightVisionOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> ShaderId = "NightVision";

    [Dependency] private IPrototypeManager _prototypeManager = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _nightVisionShader;

    private Color _tintColor = Color.White;

    public NightVisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _nightVisionShader = _prototypeManager.Index(ShaderId).InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        _nightVisionShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        var handle = args.WorldHandle;
        handle.UseShader(_nightVisionShader);
        handle.DrawRect(args.WorldBounds, _tintColor);
        handle.UseShader(null);
    }

    /// <summary>
    ///     Sets the tint color of this overlay.
    /// </summary>
    /// <param name="tint">The tint color.</param>
    public void SetColorTint(Color tint)
    {
        _tintColor = tint;
    }

    public void SetCurve(float? lowCurve, float? midCurve, float? highCurve, float? curveAmount)
    {
        if (lowCurve != null)
            _nightVisionShader.SetParameter("low_curve", lowCurve.Value);

        if (midCurve != null)
            _nightVisionShader.SetParameter("mid_curve", midCurve.Value);

        if (highCurve != null)
            _nightVisionShader.SetParameter("high_curve", highCurve.Value);

        if (curveAmount != null)
            _nightVisionShader.SetParameter("curve_amount", curveAmount.Value);
    }
}
