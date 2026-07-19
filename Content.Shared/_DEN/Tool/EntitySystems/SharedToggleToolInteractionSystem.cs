using Content.Shared.Actions;
using Content.Shared.CombatMode;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tools.Components;

namespace Content.Shared._DEN.Tool.Components;

public abstract partial class SharedToggleToolInteractionSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedCombatModeSystem _combatMode = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleToolInteractionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ToggleToolInteractionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ToggleToolInteractionComponent, ToggleToolInteractionActionEvent>(OnPerformAction);
        SubscribeLocalEvent<ToggleToolInteractionComponent, UserInteractHandEvent>(OnUserInteractHand);
    }

    private void OnMapInit(Entity<ToggleToolInteractionComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ToolToggleActionEntity, ent.Comp.ToolToggleAction);
        Dirty(ent);
    }

    private void OnShutdown(Entity<ToggleToolInteractionComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ToolToggleActionEntity);
    }

    private void OnPerformAction(Entity<ToggleToolInteractionComponent> ent, ref ToggleToolInteractionActionEvent args)
    {
        if (args.Handled)
            return;

        var value = !ent.Comp.IsToolEnabled;
        SetToolInteractionEnabled(ent.AsNullable(), value);

        // Display message when toggled.
        var msgLocId = value ? ent.Comp.ToggleOnText : ent.Comp.ToggleOffText;
        if (msgLocId != null)
            _popup.PopupClient(Loc.GetString(msgLocId),
                args.Performer,
                args.Performer);

        args.Handled = true;
    }

    private void OnUserInteractHand(Entity<ToggleToolInteractionComponent> ent, ref UserInteractHandEvent args)
    {
        if (args.Handled || !ent.Comp.IsToolEnabled || !HasComp<ToolComponent>(ent.Owner))
            return;

        _interaction.InteractUsing(ent.Owner,
            ent.Owner,
            args.Target,
            Transform(args.Target).Coordinates);

        args.Handled = true;
    }

    /// <summary>
    ///     Sets whether or not this entity can use their empty-hand interactions to perform tool actions.
    /// </summary>
    /// <param name="ent">The entity to set tool interaction status.</param>
    /// <param name="enabled">If tool interactions should be enabled.</param>
    public void SetToolInteractionEnabled(Entity<ToggleToolInteractionComponent?> ent, bool enabled)
    {
        if (!Resolve(ent.Owner, ref ent.Comp)
            || ent.Comp.IsToolEnabled == enabled)
            return;

        ent.Comp.IsToolEnabled = enabled;
        Dirty(ent);

        if (ent.Comp.ToolToggleActionEntity != null)
            _actions.SetToggled(ent.Comp.ToolToggleActionEntity, enabled);

        if (enabled && TryComp<CombatModeComponent>(ent.Owner, out var combatMode))
            _combatMode.SetInCombatMode(ent.Owner, false, combatMode);
    }
}

/// <summary>
///     Event fired when the "toggle tool interaction" action is used.
/// </summary>
public sealed partial class ToggleToolInteractionActionEvent : InstantActionEvent;
