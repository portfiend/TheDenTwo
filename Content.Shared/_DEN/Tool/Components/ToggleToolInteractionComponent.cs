using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Tool.Components;

/// <summary>
///     A component that gives a mob the ability to use a tool quality with their empty-hand interaction.
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
    ///     The entity to use for the toggle action.
    /// </summary>
    [DataField]
    public EntProtoId ToolToggleAction = "ActionToolInteractionToggle";

    /// <summary>
    ///     The entity representing the toggle action.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ToolToggleActionEntity;

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
}
