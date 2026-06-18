using Robust.Shared.GameStates;

namespace Content.Shared._DEN.Overlays.Components;

/// <summary>
///     When applied to an entity or a clothing item, this gives the entity (or wearer)
///     a "night vision" shader. This shader allows you to see better in dark environments.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NightVisionOverlayComponent : Component
{
    public readonly static Color DefaultTintColor = Color.FromHex("#888");

    /// <summary>
    ///     What color to tint the night vision effect.
    /// </summary>
    [DataField]
    public Color TintColor = DefaultTintColor;

    /// <summary>
    ///     The minimum bound of lightness that this shader will begin rebalancing values.
    /// </summary>
    [DataField]
    public float? LowCurve = null;

    /// <summary>
    ///     The point at which the "curve" is highest - what lightness increases the curve the most.
    /// </summary>
    [DataField]
    public float? MidCurve = null;

    /// <summary>
    ///     The upper bound of lightness that this shader will begin rebalancing values.
    /// </summary>
    [DataField]
    public float? HighCurve = null;

    /// <summary>
    ///     The amount that lightness values will be increased at its peak.
    /// </summary>
    [DataField]
    public float? CurveAmount = null;
}
