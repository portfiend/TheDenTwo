namespace Content.Shared._DEN.Flash.Components;

/// <summary>
///     Entities with this component will modify the stats of an incoming flash.
/// </summary>
[RegisterComponent]
public sealed partial class FlashedModifierComponent : Component
{
    /// <summary>
    ///     A multiplier to the duration of flashes received by this entity.
    /// </summary>
    [DataField("durationMod")]
    public float FlashDurationModifier = 1.0f;

    /// <summary>
    ///     A motiplier to the stun duration of flashes received by this entity.
    /// </summary>
    [DataField("stunMod")]
    public float StunDurationModifier = 1.0f;

    /// <summary>
    ///     A multiplier to the movement speed of this entity when they are flashed.
    /// </summary>
    [DataField("speedMod")]
    public float SpeedModifier = 1.0f;
}
