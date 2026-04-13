using System.Diagnostics.CodeAnalysis;
using Content.Server.Station.Systems;
using Content.Shared._DEN.Glimmer.Components;
using Content.Shared._DEN.Glimmer.EntitySystems;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Server._DEN.Glimmer.EntitySystems;

/// <summary>
///     Glimmer is a measure of "noospheric instability", a value that affects both the frequency
///     and severity of paranormal-themed events. It also affects the level of connection that
///     psionic users have with the noosphere - this is to say, powers scaling with glimmer.
/// </summary>
public sealed partial class GlimmerTrackerSystem : SharedGlimmerTrackerSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GlimmerTrackerComponent, ComponentStartup>(OnGlimmerTrackerStartup);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="args"></param>
    private void OnGlimmerTrackerStartup(Entity<GlimmerTrackerComponent> ent, ref ComponentStartup args)
    {
        var comp = ent.Comp;
        var min = comp.StartingGlimmerLevelRange.X;
        var max = comp.StartingGlimmerLevelRange.Y;
        var level = _random.Next(min, max);

        SetGlimmerLevel(ent, level);
    }

    /// <summary>
    ///     Attempt to get the closest applicable glimmer tracker for a given entity.
    /// </summary>
    /// <remarks>
    ///     The entity itself will be checked if it is a glimmer tracker, then the entity's station,
    ///     then all trackers will be iterated over to find the closest applicable tracker.
    /// </remarks>
    /// <param name="uid">The entity to retrieve the nearest valid glimmer tracker for.</param>
    /// <param name="trackerEnt">A glimmer tracker that applies to this entity.</param>
    /// <returns>Whether or not we successfully retrieved a glimmer tracker.</returns>
    private bool TryGetClosestGlimmerTracker(EntityUid uid,
        [NotNullWhen(true)] out Entity<GlimmerTrackerComponent>? trackerEnt)
    {
        // This entity is a glimmer tracker
        if (TryComp<GlimmerTrackerComponent>(uid, out var tracker))
        {
            trackerEnt = (uid, tracker);
            return true;
        }

        // This is a station map and the station has a glimmer tracker
        var xform = Transform(uid);
        if (_station.GetStationInMap(xform.MapID) is EntityUid station
            && TryComp(station, out tracker))
        {
            trackerEnt = (station, tracker);
            return true;
        }

        // We get the closest glimmer tracker in the same map
        var query = EntityQueryEnumerator<GlimmerTrackerComponent>();
        (Entity<GlimmerTrackerComponent> Tracker, float Distance)? closestTracker = null;
        while (query.MoveNext(out var trackerUid, out tracker))
        {
            var ent = (trackerUid, tracker);
            var trackerXform = Transform(trackerUid);

            if (xform.Coordinates.TryDistance(EntityManager, trackerXform.Coordinates, out var distance)
                && distance > closestTracker?.Distance)
                closestTracker = (ent, distance);

            // Global trackers do not need to be in the same map to be detected
            else if (tracker.Global)
                closestTracker ??= (ent, float.MaxValue);
        }

        // May still be null, if there are no trackers at all
        trackerEnt = closestTracker?.Tracker;
        return trackerEnt != null;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="tracker"></param>
    /// <param name="glimmer"></param>
    private void SetGlimmer(Entity<GlimmerTrackerComponent> tracker, FixedPoint2 glimmer)
    {
        var comp = tracker.Comp;
        comp.CurrentGlimmer = glimmer;

        // We're adding 1 to this, so that 0 glimmer becomes level 1.
        var glimmerDivisor = (glimmer + 1) / comp.GlimmerPerLevel;
        var glimmerLevel = Math.Floor(glimmerDivisor.Float());
        comp.CurrentGlimmerLevel = (int)glimmerLevel;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="tracker"></param>
    /// <param name="level"></param>
    private void SetGlimmerLevel(Entity<GlimmerTrackerComponent> tracker, int level)
    {
        var comp = tracker.Comp;
        comp.CurrentGlimmerLevel = level;

        var min = (level - 1) * comp.GlimmerPerLevel; // INCLUSIVE
        var max = level * comp.GlimmerPerLevel; // EXCLUSIVE
        var newGlimmer = _random.NextFloat(min.Float(), max.Float());
        comp.CurrentGlimmer = FixedPoint2.New(newGlimmer);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="tracker"></param>
    /// <returns></returns>
    [PublicAPI]
    public FixedPoint2 GetCurrentGlimmer(Entity<GlimmerTrackerComponent> tracker)
    {
        return tracker.Comp.CurrentGlimmer;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [PublicAPI]
    public FixedPoint2 GetCurrentGlimmer(EntityUid uid)
    {
        if (!TryGetClosestGlimmerTracker(uid, out var tracker))
            return FixedPoint2.Zero;

        return GetCurrentGlimmer(tracker.Value);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="tracker"></param>
    /// <returns></returns>
    [PublicAPI]
    public int GetCurrentGlimmerLevel(Entity<GlimmerTrackerComponent> tracker)
    {
        return tracker.Comp.CurrentGlimmerLevel;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [PublicAPI]
    public int GetCurrentGlimmerLevel(EntityUid uid)
    {
        if (!TryGetClosestGlimmerTracker(uid, out var tracker))
            return 0;

        return GetCurrentGlimmerLevel(tracker.Value);
    }
}
