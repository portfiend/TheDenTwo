using Content.Shared.Actions;
using Content.Shared.Body;
using Content.Shared.CombatMode;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Shared._DEN.Tool.Components;

/// <summary>
///     System logic for organs that contain pseudo-tools.
///     Using these tools is toggled via action.
/// </summary>
public abstract partial class SharedToolOrganSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private SharedCombatModeSystem _combatMode = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, GetToolOrganEvent>(_body.RelayEvent);
    }

    [SubscribeLocalEvent]
    private void OnComponentInit(Entity<ToolOrganComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ItemContainer = _container.EnsureContainer<Container>(ent.Owner, ent.Comp.ContainerId);

        var toolEnt = PredictedSpawnAtPosition(ent.Comp.Item, Transform(ent.Owner).Coordinates);
        if (!_container.Insert(toolEnt, ent.Comp.ItemContainer))
        {
            DebugTools.Assert($"Could not insert {ToPrettyString(toolEnt)} into {ToPrettyString(ent)}. This is likely due to broken YAML!");
            PredictedDel(toolEnt);
        }
        else
            ent.Comp.ItemEntity = toolEnt;

        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnOrganGotInserted(Entity<ToolOrganComponent> ent, ref OrganGotInsertedEvent args)
    {
        _actions.AddAction(args.Target, ref ent.Comp.ToolToggleActionEntity, ent.Comp.ToolToggleAction);
        EnsureComp<ToolOrganPerformerComponent>(ent.Owner);
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnOrganRemoved(Entity<ToolOrganComponent> ent, ref OrganGotRemovedEvent args)
    {
        _actions.RemoveAction(args.Target, ent.Comp.ToolToggleActionEntity);
        TrySetToolInteractionEnabled(ent.Owner, false);
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnPerformAction(Entity<ToolOrganPerformerComponent> ent, ref ToggleToolInteractionActionEvent args)
    {
        if (args.Handled)
            return;

        var ev = new GetToolOrganEvent(Action: args.Action, Mob: ent.Owner);
        RaiseLocalEvent(ent.Owner, ref ev);

        if (ev.Handled)
            args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnGetToolOrgan(Entity<ToolOrganComponent> ent, ref BodyRelayedEvent<GetToolOrganEvent> args)
    {
        if (args.Args.Handled || args.Args.Action != ent.Comp.ToolToggleActionEntity)
            return;

        var mob = args.Args.Mob;
        var value = !ent.Comp.IsToolEnabled;
        if (!TrySetToolInteractionEnabled(ent.AsNullable(), value, mob))
            return;

        // Display message when toggled.
        var msgLocId = value ? ent.Comp.ToggleOnText : ent.Comp.ToggleOffText;
        if (msgLocId != null)
            _popup.PopupClient(Loc.GetString(msgLocId), mob, mob);

        args.Args = args.Args with { Handled = true };
    }

    /// <summary>
    ///     Sets whether or not this entity can use their empty-hand interactions to perform tool actions.
    /// </summary>
    /// <param name="ent">The entity to set tool interaction status.</param>
    /// <param name="enabled">If tool interactions should be enabled.</param>
    public bool TrySetToolInteractionEnabled(Entity<ToolOrganComponent?> ent, bool enabled, EntityUid? mob = null)
    {
        if (!Resolve(ent.Owner, ref ent.Comp) // Not a tool organ
            || ent.Comp.IsToolEnabled == enabled // Value is already set to this
            || ent.Comp.ItemEntity == null // No tool entity
            || TerminatingOrDeleted(ent.Comp.ItemEntity) // Tool entity is being deleted
            || !TryComp<OrganComponent>(ent.Owner, out var organ) // Not an organ
            || organ.Body == null) // Not attached to a mob
            return false;

        mob ??= organ.Body.Value;
        var toolEnt = ent.Comp.ItemEntity.Value;

        // Put the entity in your hands as a virtual entity
        if (enabled && !_virtualItem.TrySpawnVirtualItemInHand(toolEnt, mob.Value, dropOthers: false))
            return false;
        else if (!enabled)
            _virtualItem.DeleteInHandsMatching(mob.Value, toolEnt);

        // Set the enabled state of the tool
        ent.Comp.IsToolEnabled = enabled;
        Dirty(ent);

        if (ent.Comp.ToolToggleActionEntity != null)
            _actions.SetToggled(ent.Comp.ToolToggleActionEntity, enabled);

        if (enabled && TryComp<CombatModeComponent>(mob, out var combatMode))
            _combatMode.SetInCombatMode(mob.Value, false, combatMode);

        return true;
    }
}

/// <summary>
///     Event fired when the "toggle tool interaction" action is used.
/// </summary>
public sealed partial class ToggleToolInteractionActionEvent : InstantActionEvent
{ }

/// <summary>
///     Event fired to attempt to get the appropriate tool entity when it is toggled on.
/// </summary>
/// <param name="Mob">The mob that is using this tool entity.</param>
/// <param name="Action">The action that triggered this.</param>
/// <param name="Handled">Whether or not this event has been handled.</param>
[ByRefEvent]
public record struct GetToolOrganEvent(EntityUid Action, EntityUid Mob, bool Handled = false);
