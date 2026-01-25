using Content.Shared._DEN.Traits.EntitySystems;
using Content.Shared.Traits;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.Components;
#pragma warning restore IDE1006 // Naming Styles

/// <summary>
/// A container for trait entities, allowing traits to hold their own components and state.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedTraitSystem))]
public sealed partial class TraitHolderComponent : Component
{
    public const string ContainerId = "traits";

    /// <summary>
    /// A container holding entities representing this entity's traits.
    /// </summary>
    [ViewVariables]
    public Container? Traits;
}
