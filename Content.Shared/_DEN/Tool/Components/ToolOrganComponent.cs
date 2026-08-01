using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Tool.Components;

/// <summary>
///     An organ that contains an entity representing a specific capability of the organ.
///     For example: giving an organ a "sharp" tool if you want this organ to be usable for cutting.
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ToolOrganComponent : Component
{
    /// <summary>
    ///     Whether or not tool interactions are enabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsToolEnabled = false;

    /// <summary>
    ///     The string ID used for the item container.
    /// </summary>
    [DataField]
    public string ContainerId = "toggleable_tool";

    /// <summary>
    ///     Text displayed when the action is enabled.
    /// </summary>
    [DataField]
    public LocId? ToggleOnText = "action-popup-tool-interaction-enabled";

    /// <summary>
    ///     Text displayed when the action is disabled.
    /// </summary>
    [DataField]
    public LocId? ToggleOffText = "action-popup-tool-interaction-disabled";

    /// <summary>
    ///     The entity to use for the toggle action.
    /// </summary>
    [DataField]
    public EntProtoId ToolToggleAction = "ActionToolInteractionToggle";

    /// <summary>
    ///     The entity to put in the mob's hand when this is toggled.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Item = string.Empty;

    /// <summary>
    ///     The entity representing the toggle action.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ToolToggleActionEntity;

    /// <summary>
    ///     The container holding the tool item.
    /// </summary>
    [ViewVariables]
    public Container? ItemContainer;

    /// <summary>
    ///     The entity representing the toggleable item.
    /// </summary>
    [ViewVariables]
    public EntityUid? ItemEntity;
}
