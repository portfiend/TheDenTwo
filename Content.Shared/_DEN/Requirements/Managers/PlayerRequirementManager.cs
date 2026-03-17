using Content.Shared._DEN.Requirements.PlayerRequirements;
using JetBrains.Annotations;

namespace Content.Shared._DEN.Requirements.Managers;

public sealed partial class PlayerRequirementManager
{
    /// <summary>
    ///     Check a context against enumerable requirements and gets the final pass/fail status of these requirements.
    /// </summary>
    /// <param name="context">The context containing fields to check against the requirements.</param>
    /// <param name="requirements">An enumerable collection of requirements.</param>
    /// <returns>Whether or not this context passes *all* requirements. If even one fails, then this is false.</returns>
    [PublicAPI]
    public static bool CheckRequirements(PlayerRequirementContext context, IEnumerable<IPlayerRequirement> requirements)
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
}
