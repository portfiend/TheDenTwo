using System.Diagnostics.CodeAnalysis;
using Content.Shared._DEN.Requirements.Managers;
using Content.Shared.CCVar;
using Content.Shared.Localizations;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Requirements.PlayerRequirements;

/// <summary>
///     An abstract class for playtime requirements that expect a playtime to be within
///     optional minimum and maximum parameters.
/// </summary>
public abstract partial class PlayerPlaytimeRequirement : PlayerRequirement, IPlayerRangeRequirement<TimeSpan>
{
    /// <summary>
    ///     The minimum time you can have in this tracker.
    /// </summary>
    [DataField("minTime")]
    public TimeSpan? Min { get; set; } = null;

    /// <summary>
    ///     The maximum time you can have in this tracker.
    /// </summary>
    [DataField("maxTime")]
    public TimeSpan? Max { get; set; } = null;

    /// <summary>
    ///     The "type" of playtime requirement this is.
    ///     This affects what CVAR is used to turn the timer off.
    /// </summary>
    [DataField]
    public PlaytimeRequirementType RequirementType = PlaytimeRequirementType.Role;

    /// <inheritdoc/>
    public override bool PreCheck(PlayerRequirementContext context)
    {
        // We are always returning "true" if ShouldAutoPass() is true, because otherwise, if the
        // pre-check failed, then it would be possible to fail this requirement as per
        // PlayerRequirement.MustPassPreCheck even when role timers should be ignored anyway.
        return ShouldAutoPass() || context.Playtimes != null;
    }

    /// <summary>
    ///     Whether or not this requirement should auto-pass. This applies if role timers
    ///     are disabled, because playtimes shouldn't matter anyway in this case - we shouldn't
    ///     fail playtime requirements ever when role timers are disabled.
    /// </summary>
    /// <returns>
    ///     Whether or not this requirement should auto-pass.
    /// </returns>
    public bool ShouldAutoPass()
    {
        var config = IoCManager.Resolve<IConfigurationManager>();
        var timerEnabled = RequirementType switch
        {
            PlaytimeRequirementType.Role => config.GetCVar(CCVars.GameRoleTimers),
            PlaytimeRequirementType.Loadout => config.GetCVar(CCVars.GameRoleLoadoutTimers),
            _ => throw new ArgumentOutOfRangeException(nameof(RequirementType)),
        };

        return !timerEnabled;
    }

    /// <summary>
    ///     Format a playtime TimeSpan into text to display to the player.
    /// </summary>
    /// <param name="playtime">The playtime to format.</param>
    /// <returns>The formatted playtime, if playtime is not null.</returns>
    private static string? FormatPlaytime(TimeSpan? playtime)
    {
        if (playtime is null)
            return null;

        var playtimeString = ContentLocalizationManager.FormatPlaytime(playtime.Value);
        return Loc.GetString("player-requirement-format-time",
            ("playtime", playtimeString));
    }

    /// <inheritdoc/>
    public string? GetMinText()
    {
        return FormatPlaytime(Min);
    }

    /// <inheritdoc/>
    public string? GetMaxText()
    {
        return FormatPlaytime(Max);
    }

    /// <summary>
    ///     Check if the given playtime is in range.
    /// </summary>
    /// <param name="playtime">The playtime to check.</param>
    /// <returns>Whether or not the playtime is in range.</returns>
    protected bool IsInRange(TimeSpan playtime)
    {
        if (this is IPlayerRangeRequirement<TimeSpan> range)
            return range.IsInRange(playtime);

        return false;
    }

    /// <summary>
    ///     Get the text to display to the player that represents the range of valid playtimes.
    /// </summary>
    /// <param name="playtimeString">The playtime range description.</param>
    /// <returns>Whether or not this operation was successful.</returns>
    protected bool TryGetRangeConstraintReason([NotNullWhen(true)] out string? playtimeString)
    {
        playtimeString = null;

        if (this is IPlayerRangeRequirement<TimeSpan> range)
            playtimeString = range.GetRangeConstraintReason();

        return playtimeString != null;
    }
}

[Serializable]
public enum PlaytimeRequirementType
{
    Role,
    Loadout
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

        return IsInRange(playtime.Value);
    }

    /// <inheritdoc/>
    public override string? GetReason(PlayerRequirementContext? context = null)
    {
        // Do not give a reason if role timers are disabled.
        if (ShouldAutoPass())
            return null;

        // Get a formatted department name.
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var deptName = FormatDepartment(protoMan);

        // Get the playtime constraint string.
        if (!TryGetRangeConstraintReason(out var playtimeString))
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

        return IsInRange(playtime.Value);
    }

    /// <inheritdoc/>
    public override string? GetReason(PlayerRequirementContext? context = null)
    {
        // Do not give a reason if role timers are disabled.
        if (ShouldAutoPass())
            return null;

        // Get the job name and format it with a color.
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var jobName = FormatJob(protoMan);

        // Get the playtime constraint string.
        if (!TryGetRangeConstraintReason(out var playtimeString))
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

/// <summary>
///     Checks if a player's total overall playtime fits within a given playtime range.
/// </summary>
public sealed partial class PlayerOverallPlaytimeRequirement : PlayerPlaytimeRequirement
{
    /// <inheritdoc/>
    public override bool CheckRequirement(PlayerRequirementContext context)
    {
        // Auto-pass if role timers are disabled.
        if (ShouldAutoPass())
            return true;

        var playtime = GetOverallPlaytime(context);
        if (playtime is null)
            return false;

        return IsInRange(playtime.Value);
    }

    /// <summary>
    ///     Get the overall playtime for this context.
    /// </summary>
    /// <param name="context">The context being used for checking this requirement.</param>
    /// <returns>The player's overall playtime.</returns>
    private static TimeSpan? GetOverallPlaytime(PlayerRequirementContext context)
    {
        var overallTracker = PlayTimeTrackingShared.TrackerOverall;
        var playtime = TimeSpan.Zero;

        if (context.Playtimes == null)
            return null;

        if (context.Playtimes.TryGetValue(overallTracker, out var tracker))
            playtime = tracker;

        return playtime;
    }

    /// <inheritdoc/>
    public override string? GetReason(PlayerRequirementContext? context = null)
    {
        // Do not give a reason if role timers are disabled.
        if (ShouldAutoPass())
            return null;

        // Get the playtime constraint string.
        if (!TryGetRangeConstraintReason(out var playtimeString))
            return null;

        var constraintReason = Loc.GetString("player-requirement-playtime-constraint-reason",
            ("inverted", Inverted),
            ("timeConstraint", playtimeString));

        // E.g. "You must have 300h of playtime overall."
        return Loc.GetString("player-requirement-overall-playtime-reason",
            ("constraint", constraintReason));
    }
}
