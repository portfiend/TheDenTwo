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
        {
            // Pre-check the requirement. This ensures our context has all the fields needed for the requirement.
            // If the pre-check fails, whether or not this requirement passes depends on requirement.MustPassPreCheck.
            // If you don't need to pass the pre-check, then it's an auto-success.
            if (!requirement.PreCheck(context))
            {
                if (requirement.MustPassPreCheck)
                    return false;

                continue;
            }

            // Check the actual requirement, now.
            if (!requirement.CheckRequirement(context))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public abstract PlayerRequirementContext GetPlayerContext(ICommonSession session);
}
