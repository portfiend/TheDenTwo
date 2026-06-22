
using Content.Shared.Interaction;
using JetBrains.Annotations;

namespace Content.Shared._DEN.Body.EntitySystems;

/// <summary>
///     A system that holds APIs and logic related to giving entities the ability to drink blood.
/// </summary>
public abstract partial class SharedBloodDrinkerSystem
{
    [Dependency] private SharedInteractionSystem _interactionSystem = default!;

    /// <summary>
    ///     Returns whether or not this entity is in range to drink the blood of the target.
    /// </summary>
    /// <param name="drinker">The drinkerrrr.</param>
    /// <param name="target">The target.</param>
    [PublicAPI]
    public bool IsInBloodDrinkingRange(EntityUid drinker, EntityUid target)
    {
        return _interactionSystem.InRangeUnobstructed(drinker, target);
    }
}
