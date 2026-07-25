using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Tool.Components;

/// <summary>
///     A component that allows you to toggle using an entity in your empty hand.
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ToggleToolInteractionComponent : Component
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
    ///     The container holding the tool item.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Container? ItemContainer;

    /// <summary>
    ///     The entity representing the toggle action.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ToolToggleActionEntity;

    /// <summary>
    ///     The entity representing the toggleable item.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ItemEntity;
}
