using Content.Client.Overlays;
using Content.Shared._DEN.Overlays.Components;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;
using Robust.Shared.Utility;

namespace Content.Client._DEN.Overlays;

public sealed partial class NightVisionOverlaySystem : EquipmentHudSystem<NightVisionOverlayComponent>
{
    [Dependency] private ILightManager _lightManager = default!;
    [Dependency] private IOverlayManager _overlayMan = default!;

    private NightVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new();
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<NightVisionOverlayComponent> component)
    {
        base.UpdateInternal(component);

        _overlayMan.AddOverlay(_overlay);
        _lightManager.DrawLighting = false;

        var comps = component.Components;
        if (comps.TryFirstOrDefault(out var comp))
        {
            _overlay.SetColorTint(comp.TintColor);
            _overlay.SetCurve(comp.LowCurve, comp.MidCurve, comp.HighCurve, comp.CurveAmount);
            return;
        }

        _overlay.SetColorTint(NightVisionOverlayComponent.DefaultTintColor);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        _overlayMan.RemoveOverlay(_overlay);
        _lightManager.DrawLighting = true;
    }
}
