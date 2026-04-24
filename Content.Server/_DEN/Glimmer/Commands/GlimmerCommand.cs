using Content.Server._DEN.Glimmer.EntitySystems;
using Content.Server.Administration;
using Content.Shared._DEN.Glimmer.Components;
using Content.Shared.Administration;
using Content.Shared.FixedPoint;
using Robust.Shared.Toolshed;

namespace Content.Server._DEN.Glimmer.Commands;

[ToolshedCommand, AdminCommand(AdminFlags.VarEdit)]
public sealed partial class GlimmerCommand : ToolshedCommand
{
    private GlimmerTrackerSystem? _glimmerTracker;

    [CommandImplementation("getTracker")]
    public Entity<GlimmerTrackerComponent>? GetTracker([PipedArgument] EntityUid @target)
    {
        _glimmerTracker ??= GetSys<GlimmerTrackerSystem>();

        if (_glimmerTracker.TryGetClosestGlimmerTracker(@target, out var tracker))
            return tracker;

        return null;
    }

    [CommandImplementation("getTracker")]
    public Entity<GlimmerTrackerComponent>? GetTracker(IInvocationContext ctx)
    {
        _glimmerTracker ??= GetSys<GlimmerTrackerSystem>();

        if (ctx.Session?.AttachedEntity is not { } entity)
            return null;

        if (_glimmerTracker.TryGetClosestGlimmerTracker(entity, out var tracker))
            return tracker;

        return null;
    }

    [CommandImplementation("getValue")]
    public FixedPoint2? GetValue([PipedArgument] Entity<GlimmerTrackerComponent> @tracker)
    {
        _glimmerTracker ??= GetSys<GlimmerTrackerSystem>();
        return _glimmerTracker.GetCurrentGlimmer(@tracker);
    }

    [CommandImplementation("getValue")]
    public FixedPoint2? GetValue([PipedArgument] Entity<GlimmerTrackerComponent>? @tracker)
    {
        if (@tracker == null)
            return FixedPoint2.Zero;

        _glimmerTracker ??= GetSys<GlimmerTrackerSystem>();
        return _glimmerTracker.GetCurrentGlimmer(@tracker.Value);
    }

    [CommandImplementation("getLevel")]
    public int? GetLevel([PipedArgument] Entity<GlimmerTrackerComponent> @tracker)
    {
        _glimmerTracker ??= GetSys<GlimmerTrackerSystem>();
        return _glimmerTracker.GetCurrentGlimmerLevel(@tracker);
    }

    [CommandImplementation("getLevel")]
    public int? GetLevel([PipedArgument] Entity<GlimmerTrackerComponent>? @tracker)
    {
        if (@tracker == null)
            return 0;

        _glimmerTracker ??= GetSys<GlimmerTrackerSystem>();
        return _glimmerTracker.GetCurrentGlimmerLevel(@tracker.Value);
    }

    [CommandImplementation("setValue")]
    public Entity<GlimmerTrackerComponent> SetValue([PipedArgument] Entity<GlimmerTrackerComponent> @tracker, FixedPoint2 value)
    {
        _glimmerTracker ??= GetSys<GlimmerTrackerSystem>();
        _glimmerTracker.SetGlimmer(@tracker, value);

        return @tracker;
    }

    [CommandImplementation("setValue")]
    public Entity<GlimmerTrackerComponent>? SetValue([PipedArgument] Entity<GlimmerTrackerComponent>? @tracker, FixedPoint2 value)
    {
        if (@tracker == null)
            return @tracker;

        _glimmerTracker ??= GetSys<GlimmerTrackerSystem>();
        _glimmerTracker.SetGlimmer(@tracker.Value, value);

        return @tracker;
    }

    [CommandImplementation("setLevel")]
    public Entity<GlimmerTrackerComponent> SetLevel([PipedArgument] Entity<GlimmerTrackerComponent> @tracker, int level)
    {
        _glimmerTracker ??= GetSys<GlimmerTrackerSystem>();
        _glimmerTracker.SetGlimmerLevel(@tracker, level);

        return @tracker;
    }

    [CommandImplementation("setLevel")]
    public Entity<GlimmerTrackerComponent>? SetLevel([PipedArgument] Entity<GlimmerTrackerComponent>? @tracker, int level)
    {
        if (@tracker == null)
            return @tracker;

        _glimmerTracker ??= GetSys<GlimmerTrackerSystem>();
        _glimmerTracker.SetGlimmerLevel(@tracker.Value, level);

        return @tracker;
    }
}
