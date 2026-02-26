using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DEN.Kitchen.Prototypes;

/// <summary>
///     Defines a type of appliance that can be used for cooking purposes.
///     Recipes only need the prototype ID, but this prototype also contains metadata such as name/icon
///     for the sake of guidebook entries.
/// </summary>
[Prototype]
public sealed partial class CookingAppliancePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     The display name of this appliance in the guidebook.
    /// </summary>
    [DataField]
    public LocId Name = string.Empty;

    /// <summary>
    ///     The icon for this appliance in the guidebook.
    /// </summary>
    [DataField]
    public SpriteSpecifier? Icon = null;
}
