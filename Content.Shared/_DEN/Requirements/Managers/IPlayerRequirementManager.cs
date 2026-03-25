using Content.Shared._DEN.Requirements.PlayerRequirements;
using Robust.Shared.Player;

namespace Content.Shared._DEN.Requirements.Managers;

/// <summary>
///     A manager used to check player stats against a list of requirements, getting the pass/fail status of these requirements.
///     This can be used to apply restrictions to character actions, like jobs or traits.
/// </summary>
public interface IPlayerRequirementManager
{
    /// <summary>
    ///     Check a context against enumerable requirements and gets the final pass/fail status of these requirements.
    /// </summary>
    /// <param name="context">The context containing fields to check against the requirements.</param>
    /// <param name="requirements">An enumerable collection of requirements.</param>
    /// <returns>Whether or not this context passes *all* requirements. If even one fails, then this is false.</returns>
    bool CheckRequirements(PlayerRequirementContext context, IEnumerable<IPlayerRequirement> requirements);

    /// <summary>
    ///     Whether or not the item associated with a set of requirements should be hidden
    ///     from the player, such as in UI.
    /// </summary>
    /// <param name="context">The context containing fields to check against the requirements.</param>
    /// <param name="requirements">An enumerable collection of requirements.</param>
    /// <returns>Whether or not the item should be hidden from the player.</returns>
    bool ShouldHide(PlayerRequirementContext context, IEnumerable<IPlayerRequirement> requirements);

    /// <summary>
    ///     Creates a new PlayerRequirementContext with context fields pre-filled.
    /// </summary>
    /// <param name="session">The session associated with this player.</param>
    /// <returns>A pre-filled requirement context for this player.</returns>
    PlayerRequirementContext GetPlayerContext(ICommonSession session);
}
