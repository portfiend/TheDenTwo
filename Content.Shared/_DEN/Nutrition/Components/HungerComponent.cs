using Content.Shared.Nutrition.EntitySystems;

namespace Content.Shared.Nutrition.Components;

public sealed partial class HungerComponent : Component
{
    /// <summary>
    ///     How many tick intervals (<see cref="DropTickInterval" />) it takes to drop hunger by <see cref="HungerDropAmount"/>
    /// </summary>
    public const float HungerDropTimeInIntervals = 50.0f;

    /// <summary>
    ///     Amount of hunger lost in <see cref="HungerDropTimeInIntervals"/>.
    /// </summary>
    public const float HungerDropAmount = 50.0f;

    /// <summary>
    ///     How many ticks per "tick interval". 60 ticks at 1 tick / second = 1 minute.
    /// </summary>
    /// <remarks>
    ///     This is a bad way of doing it but the better way is a huge breaking change
    /// </remarks>
    private const float DropTickInterval = 60.0f;

    /// <summary>
    ///     The hunger rate that humanoid species are balanced around.
    /// </summary>
    private static readonly float BaseHumanoidHungerRate = HungerDropAmount / (HungerDropTimeInIntervals * DropTickInterval);

    /// <summary>
    ///     A flat multiplier applied to <see cref="BaseDecayRate"/>.
    /// </summary>
    /// <remarks>
    ///     This value ideally should not change - this is to make species code more intuitive to write.
    /// </remarks>
    [DataField, Access(typeof(HungerSystem), Friend = AccessPermissions.Read)]
    public float DecayRateMultiplier = 1.0f;
}
