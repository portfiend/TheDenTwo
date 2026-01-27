using System.Reflection;
using Content.Shared._DEN.Traits.EntitySystems;
using Content.Shared._DEN.Traits.Prototypes;
using Content.Shared._DEN.Traits.TraitFunctions;
using Robust.Shared.Prototypes;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.Components;
#pragma warning restore IDE1006 // Naming Styles

/// <summary>
/// A container for trait entities, allowing traits to hold their own components and state.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedTraitSystem))]
public sealed partial class TraitComponent : Component
{
    /// <summary>
    /// A list of functions associated with this trait. These are applied when the trait is activated.
    /// </summary>
    [DataField]
    public IReadOnlyList<ITraitFunction> TraitFunctions = new List<ITraitFunction>();

    [ViewVariables]
    public EntityUid? Holder = null;

    [ViewVariables]
    public ProtoId<EntityTraitPrototype>? Prototype = null;
}
