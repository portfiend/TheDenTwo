using Content.Shared._DEN.Flash.Components;

namespace Content.Shared.Flash;

public abstract partial class SharedFlashSystem
{
    /// <summary>
    ///     Modifies the stats of an incoming flash on this entity.
    /// </summary>
    /// <param name="ent">The entity being flashed.</param>
    private void OnFlashModified(Entity<FlashedModifierComponent> ent, ref FlashModifierEvent args)
    {
        args.FlashDurationModifier *= ent.Comp.FlashDurationModifier;
        args.StunDurationModifier *= ent.Comp.StunDurationModifier;
        args.SpeedModifier *= ent.Comp.SpeedModifier;
    }
}

/// <summary>
///     Event used to modify the stats associated with a particular flash attempt.
/// </summary>
[ByRefEvent]
public record struct FlashModifierEvent(EntityUid Target,
    EntityUid? User,
    EntityUid? Used)
{
    /// <summary>
    ///     A multiplier to the duration of flashes received by this entity.
    /// </summary>
    public float FlashDurationModifier = 1.0f;

    /// <summary>
    ///     A motiplier to the stun duration of flashes received by this entity.
    /// </summary>
    public float StunDurationModifier = 1.0f;

    /// <summary>
    ///     A multiplier to the movement speed of this entity when they are flashed.
    /// </summary>
    public float SpeedModifier = 1.0f;
}
