using Content.Shared._DEN.Body.Components;
using Content.Shared.Bed.Sleep;
using Content.Shared.DoAfter;
using Content.Shared.HealthExaminable;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using JetBrains.Annotations;
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
        var performer = args.User;
        var target = ent;
        var comp = ent.Comp;

        if (!CanConcealBiteMarks(performer, target))
            return;

        var isSelf = performer == target.Owner;
        var verb = new AlternativeVerb()
        {
            Icon = comp.VerbIcon,
            Text = Loc.GetString(comp.VerbLocId),
            Message = Loc.GetString(comp.VerbTooltipLocId),
            Priority = comp.VerbPriority,
            DoContactInteraction = !isSelf, // leaves fingerprints if target != performer
            Act = () => { StartConcealBiteMarks(target.AsNullable(), performer); }
        };

        args.Verbs.Add(verb);
    }

    /// <summary>
    ///     Remove the examine text component from a blood drinking victim after they finish the "conceal" verb.
    /// </summary>
    /// <param name="ent">The blood drinking victim.</param>
    [SubscribeLocalEvent]
    private void OnConcealBiteMarks(Entity<BloodDrinkerVictimComponent> ent, ref ConcealBiteWoundsDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        RemCompDeferred(ent.Owner, ent.Comp);

        // Play sound.
        _audio.PlayPredicted(ent.Comp.ConcealSound, source: ent, user: args.User);

        // Do popups.
        DoConcealEndPopups(args.Target.Value, args.User);

        args.Handled = true;
    }

    /// <summary>
    ///     Begin the DoAfter for removing the vampire bite examine text component.
    /// </summary>
    /// <param name="ent">The blood drinking victim.</param>
    private void StartConcealBiteMarks(Entity<BloodDrinkerVictimComponent?> ent, EntityUid performer)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        var ev = new ConcealBiteWoundsDoAfterEvent();

        // these parameters are largely arbitrary
        var doAfterArgs = new DoAfterArgs(EntityManager,
            user: performer,
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
            _audio.PlayPredicted(ent.Comp.ConcealSound, source: ent, user: performer);
            DoConcealStartPopups(ent, performer);
        }
    }

    /// <summary>
    ///     Display popup messages upon attempting to conceal a target's bite marks.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="performer">The entity concealing the target's bite marks.</param>
    private void DoConcealStartPopups(Entity<BloodDrinkerVictimComponent?> target, EntityUid performer)
    {
        if (!Resolve(target.Owner, ref target.Comp))
            return;

        // "Self" popups.
        if (performer == target.Owner)
        {
            var msg = Loc.GetString(target.Comp.SelfConcealPopupStart);
            _popup.PopupEntity(msg, target, performer);

            return;
        }

        // Popups between a performer and target.
        var performerId = Identity.Entity(performer, EntityManager);
        var targetId = Identity.Entity(target, EntityManager);
        var performerMsg = Loc.GetString(target.Comp.PerformerConcealPopupStart, ("target", targetId));
        var targetMsg = Loc.GetString(target.Comp.TargetConcealPopupStart, ("user", performerId));

        _popup.PopupEntity(performerMsg, target, recipient: performer);
        _popup.PopupEntity(targetMsg, target, recipient: target, PopupType.MediumCaution);
    }

    /// <summary>
    ///     Display popup messages upon finishing concealing a target's bite marks.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="performer">The entity concealing the target's bite marks.</param>
    private void DoConcealEndPopups(Entity<BloodDrinkerVictimComponent?> target, EntityUid performer)
    {
        if (!Resolve(target.Owner, ref target.Comp))
            return;

        // "Self" popups.
        if (performer == target.Owner)
        {
            var msg = Loc.GetString(target.Comp.SelfConcealPopupEnd);
            _popup.PopupEntity(msg, target, performer);

            return;
        }

        // Popups between a performer and target.
        var performerId = Identity.Entity(performer, EntityManager);
        var targetId = Identity.Entity(target, EntityManager);
        var performerMsg = Loc.GetString(target.Comp.PerformerConcealPopupEnd, ("target", targetId));
        var targetMsg = Loc.GetString(target.Comp.TargetConcealPopupEnd, ("user", performerId));

        _popup.PopupEntity(performerMsg, target, recipient: performer);
        _popup.PopupEntity(targetMsg, target, recipient: target, PopupType.MediumCaution);
    }

    /// <summary>
    ///     Whether or not a given entity can hide the bite marks of a target.
    /// </summary>
    /// <param name="performer">The entity attempting to hite bite marks.</param>
    /// <param name="target">The target.</param>
    [PublicAPI]
    public bool CanConcealBiteMarks(EntityUid performer, EntityUid target)
    {
        // Does the performer has the cognitive capacity to do this?
        if (_mobState.IsIncapacitated(performer) || HasComp<SleepingComponent>(performer))
            return false;

        // Very little should prevent you from concealing your own bite marks.
        // This ignores the range check because it's meant to be more of a RP preference than a mechanic thing.
        if (performer == target)
            return true;

        // Too far away. If you can't reach their neck, you can't reach their neck.
        if (!IsInBloodDrinkingRange(performer, target))
            return false;

        // They're unconscious. They're probably not gonna stop you.
        if (_mobState.IsIncapacitated(target) || HasComp<SleepingComponent>(target))
            return true;

        // The target is conscious AND sentient, so probably not.
        if (_mind.TryGetMind(target, out _, out _))
            return false;

        return true;
    }
}

/// <summary>
///     DoAfter event for attempting to remove one's vampire bite marks.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class ConcealBiteWoundsDoAfterEvent : SimpleDoAfterEvent;
