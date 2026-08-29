using Content.Shared._DEN.Body.Components;
using Content.Shared.DoAfter;
using Content.Shared.HealthExaminable;
using Content.Shared.IdentityManagement;
using Content.Shared.Verbs;
using Robust.Shared.Serialization;

namespace Content.Shared._DEN.Body.EntitySystems;

// Logic for mobs that get their blood silly straw'd

public abstract partial class SharedBloodDrinkerSystem
{
    /// <summary>
    ///     Add examine text to a blood drinking victim.
    /// </summary>
    /// <param name="ent">The blood drinking victim.</param>
    [SubscribeLocalEvent]
    private void OnVictimHealthExamined(Entity<BloodDrinkerVictimComponent> ent, ref HealthBeingExaminedEvent args)
    {
        var id = Identity.Entity(ent, EntityManager);
        var msg = Loc.GetString(ent.Comp.ExamineText, ("victim", id));

        args.Message.PushNewline();
        args.Message.AddMarkupOrThrow(msg);
    }

    /// <summary>
    ///     Adds a verb to blood-drinking victims to hide their own bite marks.
    /// </summary>
    /// <param name="ent">The blood drinking victim.</param>
    [SubscribeLocalEvent]
    private void OnVictimGetVerbs(Entity<BloodDrinkerVictimComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        // Self-verb only.
        if (args.User != ent.Owner)
            return;

        var verb = new Verb()
        {
            Icon = ent.Comp.VerbIcon,
            Text = Loc.GetString(ent.Comp.VerbLocId),
            Priority = ent.Comp.VerbPriority,
            Act = () => { StartConcealBiteMarks(ent.AsNullable()); }
        };
    }

    /// <summary>
    ///     Remove the examine text component from a blood drinking victim after they finish the "conceal" verb.
    /// </summary>
    /// <param name="ent">The blood drinking victim.</param>
    [SubscribeLocalEvent]
    private void OnConcealBiteMarks(Entity<BloodDrinkerVictimComponent> ent, ref ConcealBiteWoundsDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        RemCompDeferred(ent.Owner, ent.Comp);

        var msg = Loc.GetString(ent.Comp.ConcealPopupEnd);
        _popup.PopupEntity(msg, ent, ent);

        args.Handled = true;
    }

    /// <summary>
    ///     Begin the DoAfter for removing the vampire bite examine text component.
    /// </summary>
    /// <param name="ent">The blood drinking victim.</param>
    private void StartConcealBiteMarks(Entity<BloodDrinkerVictimComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        var ev = new ConcealBiteWoundsDoAfterEvent();

        // these parameters are largely arbitrary
        var doAfterArgs = new DoAfterArgs(EntityManager,
            user: ent,
            delay: ent.Comp.ConcealTime,
            @event: ev,
            eventTarget: ent,
            target: ent)
        {
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnDamage = true,
            MovementThreshold = 0.1f,
        };

        if (_doAfter.TryStartDoAfter(doAfterArgs))
        {
            var msg = Loc.GetString(ent.Comp.ConcealPopupStart);
            _popup.PopupEntity(msg, ent, ent);
        }
    }
}

/// <summary>
///     DoAfter event for attempting to remove one's vampire bite marks.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class ConcealBiteWoundsDoAfterEvent : SimpleDoAfterEvent;
