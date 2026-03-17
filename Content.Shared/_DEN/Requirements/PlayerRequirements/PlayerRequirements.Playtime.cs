using Content.Shared._DEN.Requirements.Managers;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Requirements.PlayerRequirements;

/// <summary>
///     An abstract class for playtime requirements that expect a playtime to be within
///     optional minimum and maximum parameters.
/// </summary>
public abstract partial class PlayerPlaytimeRequirement : IPlayerRequirement
{
    /// <inheritdoc/>
    [DataField] public bool Inverted { get; set; } = false;

    /// <inheritdoc/>
    [DataField] public bool MustPassPreCheck { get; set; } = false;

    /// <summary>
    ///     The minimum time you can have in this tracker.
    /// </summary>
    [DataField] public TimeSpan? MinTime = null;

    /// <summary>
    ///     The maximum time you can have in this tracker.
    /// </summary>
    [DataField] public TimeSpan? MaxTime = null;

    /// <inheritdoc/>
    public bool PreCheck(PlayerRequirementContext context)
    {
        return context.Playtimes != null;
    }

    /// <inheritdoc/>
    public abstract bool CheckRequirement(PlayerRequirementContext context);

    /// <inheritdoc/>
    public abstract string? GetReason(PlayerRequirementContext context);

    /// <summary>
    ///     Check if a given playtime tracker fits within the minimum and maximum times of this requirement.
    /// </summary>
    /// <param name="playtime">The playtime to check.</param>
    /// <returns>Whether or not this playtime is valid.</returns>
    protected bool IsValidPlaytime(TimeSpan playtime)
    {
        if (MinTime != null & playtime < MinTime)
            return false;

        if (MaxTime != null & playtime > MaxTime)
            return false;

        return true;
    }

    /// <summary>
    ///     Gets a localized "reason" string for this requirement's playtime ranges.
    /// </summary>
    /// <remarks>
    ///     For example: "Less than 30 minutes", "At least 120 minutes", "Between 20 minutes and 180 minutes".
    /// </remarks>
    /// <returns>
    ///     A string describing how much playtime you should have. Null if both minimum and maximum as null.
    /// </returns>
    protected string? GetPlaytimeConstraintReason()
    {
        if (MinTime == null && MaxTime == null)
            return null;

        var minTimeString = FormatPlaytime(MinTime);
        var maxTimeString = FormatPlaytime(MaxTime);

        return (MinTime, MaxTime) switch
        {
            (not null, not null) => Loc.GetString("player-requirement-playtime-minmax-time",
                ("minimum", minTimeString), ("maximum", maxTimeString)),

            (null, not null) => Loc.GetString("player-requirement-playtime-maximum-time",
                ("playtime", maxTimeString)),

            (not null, null) => Loc.GetString("player-requirement-playtime-minimum-time",
                ("playtime", minTimeString)),

            _ => null
        };
    }

    /// <summary>
    ///     Gets a localized time string for the given playtime.
    /// </summary>
    /// <param name="playtime">The playtime to format.</param>
    /// <returns>A localized time string for this playtime.</returns>
    private static string FormatPlaytime(TimeSpan? playtime)
    {
        var time = ((int?)playtime?.TotalMinutes) ?? 0;

        return playtime != null
            ? Loc.GetString("player-requirement-playtime-time", ("playtime", time))
            : string.Empty;
    }
}

/// <summary>
///     Checks if a player's total playtime in a given department fits within a given playtime range.
/// </summary>
public sealed partial class PlayerDepartmentPlaytimeRequirement : PlayerPlaytimeRequirement
{
    /// <summary>
    ///     The department we should check against the requirement.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<DepartmentPrototype> Department = default!;

    /// <inheritdoc/>
    public override bool CheckRequirement(PlayerRequirementContext context)
    {
        var playtime = GetDepartmentPlaytime(context);
        if (playtime is null)
            return false;

        return IsValidPlaytime(playtime.Value);
    }

    /// <inheritdoc/>
    public override string? GetReason(PlayerRequirementContext context)
    {
        // Get the department name.
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        if (!protoMan.TryIndex(Department, out var department))
            return null;

        var deptName = Loc.GetString(department.Name);

        // Get the playtime constraint string.
        var playtimeString = GetPlaytimeConstraintReason();
        if (playtimeString == null)
            return null;

        // E.g. "You must have 120 minutes in the Science department."
        return Loc.GetString("player-requirement-department-playtime-reason",
            ("timeConstraint", playtimeString),
            ("department", deptName));
    }

    /// <summary>
    ///     Get the total playtime for this department.
    /// </summary>
    /// <param name="context">A definition of parameters to check against the requirement.</param>
    /// <returns>The total playtime of this department. Null if either context playtimes or department is invalid.</returns>
    private TimeSpan? GetDepartmentPlaytime(PlayerRequirementContext context)
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var playtime = TimeSpan.Zero;

        if (context.Playtimes == null
            || !protoMan.TryIndex(Department, out var department))
            return null;

        // Sum the playtimes of all roles in this department.
        foreach (var roleId in department.Roles)
        {
            if (!protoMan.TryIndex(roleId, out var role))
                continue;

            if (!context.Playtimes.TryGetValue(role.PlayTimeTracker, out var roleTime))
                continue;

            playtime += roleTime;
        }

        return playtime;
    }
}
