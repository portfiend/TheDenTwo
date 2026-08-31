using Content.Shared.Body.Systems;

namespace Content.Shared._DEN.Body.Systems;

/// <summary>
///     This entity can see the bloodstream reagents of other species.
///     Note that this does not detect all chemicals in the bloodstream - just whatever their
///     actual BloodstreamComponent blood is.
/// </summary>
[RegisterComponent]
[Access(typeof(BloodstreamSystem))]
public sealed partial class BloodExaminerComponent : Component
{
    /// <summary>
    ///     Examine text for the target. Has two variables: "$target" and "$blood".
    /// </summary>
    /// <example>
    ///     You can smell {POSS-ADJ($target)} {$blood}.
    /// </example>
    [DataField]
    public LocId ExamineText = "blood-examiner-component-examine";

    /// <summary>
    ///     The word tacked onto the end of blood reagent list if it doesn't end in "blood".
    /// </summary>
    /// <example>
    ///     You can smell their sugar blood.
    /// </example>
    [DataField]
    public LocId BloodSuffix = "blood-examiner-component-blood-suffix";
}
