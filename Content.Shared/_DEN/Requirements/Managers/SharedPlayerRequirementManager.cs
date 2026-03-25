using Content.Shared._DEN.Requirements.PlayerRequirements;
using JetBrains.Annotations;
using Robust.Shared.Player;

namespace Content.Shared._DEN.Requirements.Managers;

/// <summary>
///     A manager used to check player stats against a list of requirements, getting the pass/fail status of these requirements.
///     This can be used to apply restrictions to character actions, like jobs or traits.
/// </summary>
public abstract partial class SharedPlayerRequirementManager : IPlayerRequirementManager
{
    /// <inheritdoc />
    [PublicAPI]
    public bool CheckRequirements(PlayerRequirementContext context, IEnumerable<IPlayerRequirement> requirements)
    {
        foreach (var requirement in requirements)
            if (!CheckRequirement(context, requirement))
                return false;

        return true;
    }

    /// <inheritdoc />
    [PublicAPI]
    public bool ShouldHide(PlayerRequirementContext context, IEnumerable<IPlayerRequirement> requirements)
    {
        foreach (var requirement in requirements)
            if (!CheckRequirement(context, requirement) && requirement.HideIfFailed)
                return true;

        return false;
    }

    /// <summary>
    ///     Check a single requirement for whether it passes/fails against a context.
    /// </summary>
    /// <param name="context">The context containing fields to check against the requirement.</param>
    /// <param name="requirements">The requirement to check.</param>
    /// <returns>Whether this context passes the requirement.</returns>
    [PublicAPI]
    public static bool CheckRequirement(PlayerRequirementContext context, IPlayerRequirement requirement)
    {
        // Pre-check the requirement. This ensures our context has all the fields needed for the requirement.
        // If the pre-check fails, whether or not this requirement passes depends on requirement.MustPassPreCheck.
        // If you don't need to pass the pre-check, then it's an auto-success.
        if (!requirement.PreCheck(context))
            return !requirement.MustPassPreCheck;

        // Check the actual requirement, now.
        if (!requirement.CheckRequirement(context))
            return false;

        return true;
    }

    /// <inheritdoc />
    public abstract PlayerRequirementContext GetPlayerContext(ICommonSession session);
}
