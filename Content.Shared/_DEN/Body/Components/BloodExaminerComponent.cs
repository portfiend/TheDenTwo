using Content.Shared.Body.Systems;

namespace Content.Shared._DEN.Body.Systems;

/// <summary>
///     This entity can see the bloodstream reagents of other species.
///     Note that this does not detect all chemicals in the bloodstream - just whatever their
///     actual BloodstreamComponent blood is.
/// </summary>
[RegisterComponent, Access(typeof(SharedBloodstreamSystem))]
public sealed partial class BloodExaminerComponent : Component
{
    [DataField]
    public LocId ExamineText = "blood-examiner-component-examine";

    [DataField]
    public LocId BloodSuffix = "blood-examiner-component-blood-suffix";
}
