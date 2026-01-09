
#pragma warning disable IDE1006 // Naming Styles
using Content.Shared._DEN.Species.EntitySystems;

namespace Content.Shared._DEN.Species.Components;
#pragma warning restore IDE1006 // Naming Styles

/// <summary>
/// This component adds examinable text to an entity, indicating what kind of "physiology" they have.
/// This is primarily for humanoid species to indicate the base species and "morph" of a given character.
/// For example: "They have {physiology} physiology."
///     -> physiology: {prefix} {base}
///     -> "He has felinid humanoid physiology."
/// </summary>
/// <remarks>
/// Ideally, species should be designed so that the base species mob has a base label representing the species,
/// and then the prefix is provided by a "morph trait".
/// </remarks>
[RegisterComponent]
[Access(typeof(SharedPhysiologyDescriptionSystem))]
public sealed partial class PhysiologyDescriptionComponent : Component
{
    /// <summary>
    /// A label for the base physiology of an entity; e.g. "humanoid", "reptilian", "moth".
    /// </summary>
    [DataField(required: true)]
    public LocId BaseLabel = default!;

    /// <summary>
    /// A label for the "morph" or variant of an entity; e.g. "felinid" humanoid, "draconic" reptilian.
    /// </summary>
    [DataField]
    public LocId? PrefixLabel = null;

    /// <summary>
    /// The text displayed on this entity's examine tooltip.
    /// </summary>
    [DataField]
    public LocId ExamineText = "physiology-description-examine-text";

    /// <summary>
    /// The descriptor for an entity without a prefix.
    /// </summary>
    [DataField]
    public LocId BasePhysiologyDescriptor = "physiology-description-examine-physiology";

    /// <summary>
    /// The descriptor for an entity with a prefix.
    /// </summary>
    [DataField]
    public LocId PrefixedPhysiologyDescriptor = "physiology-description-examine-physiology-prefix";
}
