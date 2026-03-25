using Content.Shared._DEN.Requirements.Managers;
using Content.Shared.CCVar;
using Content.Shared.Localizations;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Requirements.PlayerRequirements;

/// <summary>
///     An abstract class for playtime requirements that expect a playtime to be within
///     optional minimum and maximum parameters.
/// </summary>
public abstract partial class PlayerPlaytimeRequirement : PlayerRequirement
{
    /// <summary>
    ///     The minimum time you can have in this tracker.
    /// </summary>
    [DataField] public TimeSpan? MinTime = null;

    /// <summary>
    ///     The maximum time you can have in this tracker.
    /// </summary>
    [DataField] public TimeSpan? MaxTime = null;

    /// <inheritdoc/>
    public override bool PreCheck(PlayerRequirementContext context)
    {
        // We are always returning "true" if ShouldAutoPass() is true, because otherwise, if the
        // pre-check failed, then it would be possible to fail this requirement as per
        // PlayerRequirement.MustPassPreCheck even when role timers should be ignored anyway.
        return ShouldAutoPass() || context.Playtimes != null;
    }

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
    ///     Whether or not this requirement should auto-pass. This applies if role timers
    ///     are disabled, because playtimes shouldn't matter anyway in this case - we shouldn't
    ///     fail playtime requirements ever when role timers are disabled.
    /// </summary>
    /// <returns>
    ///     Whether or not this requirement should auto-pass.
    /// </returns>
    protected static bool ShouldAutoPass()
    {
        var config = IoCManager.Resolve<IConfigurationManager>();
        return !config.GetCVar(CCVars.GameRoleTimers);
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

        return (minTimeString, maxTimeString) switch
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

    private static string? FormatPlaytime(TimeSpan? playtime)
    {
        if (playtime is null)
            return null;

        var playtimeString = ContentLocalizationManager.FormatPlaytime(playtime.Value);
        return Loc.GetString("player-requirement-format-time",
            ("playtime", playtimeString));
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
        // Auto-pass if role timers are disabled.
        if (ShouldAutoPass())
            return true;

        var playtime = GetDepartmentPlaytime(context);
        if (playtime is null)
            return false;

        return IsValidPlaytime(playtime.Value);
    }

    /// <inheritdoc/>
    public override string? GetReason()
    {
        // Do not give a reason if role timers are disabled.
        if (ShouldAutoPass())
            return null;

        // Get a formatted department name.
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var deptName = FormatDepartment(protoMan);

        // Get the playtime constraint string.
        var playtimeString = GetPlaytimeConstraintReason();
        if (playtimeString == null)
            return null;

        var constraintReason = Loc.GetString("player-requirement-playtime-constraint-reason",
            ("inverted", Inverted),
            ("timeConstraint", playtimeString));

        // E.g. "You must have 2h30m of playtime in the Science department."
        return Loc.GetString("player-requirement-department-playtime-reason",
            ("constraint", constraintReason),
            ("department", deptName));
    }

    /// <summary>
    ///     Format this requirement's department name, with a color.
    /// </summary>
    /// <param name="protoMan">The prototype manager.</param>
    /// <returns>The department name of this prototype, formatted.</returns>
    private string FormatDepartment(IPrototypeManager protoMan)
    {
        if (!protoMan.TryIndex(Department, out var department))
            return Department;

        var deptName = Loc.GetString(department.Name);
        var deptColor = department.Color.ToHex();
        var formattedDept = Loc.GetString("player-requirement-format-department",
            ("color", deptColor),
            ("department", deptName));

        return formattedDept;
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

/// <summary>
///     Checks if a player's total playtime in a given job fits within a given playtime range.
/// </summary>
public sealed partial class PlayerJobPlaytimeRequirement : PlayerPlaytimeRequirement
{
    /// <summary>
    ///     The job we should check against the requirement.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<JobPrototype> Job = default!;

    /// <inheritdoc/>
    public override bool CheckRequirement(PlayerRequirementContext context)
    {
        // Auto-pass if role timers are disabled.
        if (ShouldAutoPass())
            return true;

        var playtime = GetJobPlaytime(context);
        if (playtime is null)
            return false;

        return IsValidPlaytime(playtime.Value);
    }

    /// <inheritdoc/>
    public override string? GetReason()
    {
        // Do not give a reason if role timers are disabled.
        if (ShouldAutoPass())
            return null;

        // Get the job name and format it with a color.
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var jobName = FormatJob(protoMan);

        // Get the playtime constraint string.
        var playtimeString = GetPlaytimeConstraintReason();
        if (playtimeString == null)
            return null;

        var constraintReason = Loc.GetString("player-requirement-playtime-constraint-reason",
            ("inverted", Inverted),
            ("timeConstraint", playtimeString));

        // E.g. "You must have 2h30m of playtime as a Mime."
        return Loc.GetString("player-requirement-job-playtime-reason",
            ("constraint", constraintReason),
            ("job", jobName));
    }

    /// <summary>
    ///     Format this requirement's job name, with a color.
    /// </summary>
    /// <param name="protoMan">The prototype manager.</param>
    /// <returns>The department name of this prototype, formatted.</returns>
    private string FormatJob(IPrototypeManager protoMan)
    {
        if (!protoMan.TryIndex(Job, out var job))
            return Job;

        var jobName = Loc.GetString(job.Name);

        // Gotta use the department to recolor this role's name.
        var entMan = IoCManager.Resolve<EntityManager>();
        var jobSystem = entMan.System<SharedJobSystem>();
        var deptColor = Color.LightGray.ToHex();
        if (jobSystem.TryGetPrimaryDepartment(Job, out var dept) || jobSystem.TryGetDepartment(Job, out dept))
            deptColor = dept.Color.ToHex();

        var formattedJob = Loc.GetString("player-requirement-format-job",
            ("color", deptColor),
            ("job", jobName));

        return formattedJob;
    }

    /// <summary>
    ///     Get the total playtime for this job.
    /// </summary>
    /// <param name="context">A definition of parameters to check against the requirement.</param>
    /// <returns>The total playtime of this job. Null if either context playtimes or job is invalid.</returns>
    private TimeSpan? GetJobPlaytime(PlayerRequirementContext context)
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var playtime = TimeSpan.Zero;

        if (context.Playtimes == null || !protoMan.TryIndex(Job, out var job))
            return null;

        if (context.Playtimes.TryGetValue(job.PlayTimeTracker, out var tracker))
            playtime = tracker;

        return playtime;
    }
}
