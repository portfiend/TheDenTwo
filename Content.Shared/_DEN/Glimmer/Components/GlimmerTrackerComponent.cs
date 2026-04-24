using Content.Shared._DEN.Glimmer.EntitySystems;
using Content.Shared.FixedPoint;

namespace Content.Shared._DEN.Glimmer.Components;

/// <summary>
///     A tracker component for measuring glimmer, a measure of current noospheric
///     instability and influence. Glimmer naturally shifts and fluctuates over time, but
///     player interference may cause it to shift one way or the other.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedGlimmerTrackerSystem))]
public sealed partial class GlimmerTrackerComponent : Component
{
    /// <summary>
    ///     How many levels of glimmer should we have?
    /// </summary>
    [DataField]
    public int GlimmerLevels = 10;

    /// <summary>
    ///     How many points of glimmer are in a glimmer level threshold?
    /// </summary>
    [DataField]
    public FixedPoint2 GlimmerPerLevel = 100.0f;

    /// <summary>
    ///     What level of glimmer should we start with?
    /// </summary>
    [DataField]
    public Vector2i StartingGlimmerLevelRange = new(0, 2);

    /// <summary>
    ///     An optional limit to what range of glimmer levels we can have.
    /// </summary>
    /// <remarks>
    ///     This is meaningful because if GLs runs from 0 to 10, then capping GLs
    ///     between, say, 4 to 7, would ensure constant "moderate-to-high glimmer".
    /// </remarks>
    [DataField]
    public Vector2i? PossibleGlimmerLevelRange = null;

    /// <summary>
    ///     Whether or not glimmer can be changed via gameplay means - random
    ///     fluctuation, events, and player interference.
    /// </summary>
    /// <remarks>
    ///     Admins are always capable of changing the current level or amount of glimmer.
    /// </remarks>
    [DataField]
    public bool AllowGlimmerChange = true;

    /// <summary>
    ///     Whether or not this tracker can be used by other maps.
    /// </summary>
    [DataField]
    public bool Global = false;

    /// <summary>
    ///     Whether or not actions that happen off the map this entity is contained in
    ///     can affect this glimmer tracker.
    /// </summary>
    /// <remarks>
    ///     For example: if this is disabled, then psionics usage at CentComm will not
    ///     affect the station glimmer tracker.
    /// </remarks>
    [DataField]
    public bool OffMapAffectsGlimmer = false;

    /// <summary>
    ///     How much glimmer do we currently have?
    /// </summary>
    /// <remarks>
    ///     Note that setting glimmer directly is not super meaningful or long-term;
    ///     glimmer frequently fluctuates.
    /// </remarks>
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 CurrentGlimmer = 0;

    /// <summary>
    ///     What is our current glimmer level?
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int CurrentGlimmerLevel = 0;

    /// <remarks>
    ///     Glimmer is represented as a range from 0 to (GlimmerLevels * GlimmerPerLevel).
    ///     Assuming there are 10 levels with 100 glimmer each: [0-100) glimmer is level 1,
    ///     [100-200) glimmer is level 2, et cetera. 1000 glimmer is level 10.
    /// </remarks>
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 MaxGlimmer => GlimmerPerLevel * GlimmerLevels;
}
