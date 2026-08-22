using System.Diagnostics.CodeAnalysis;
using Content.Shared._DEN.Body.Components;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Nutrition.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared._DEN.Body.EntitySystems;

/// <summary>
///     A system that holds APIs and logic related to giving entities the ability to drink blood.
/// </summary>
public abstract partial class SharedBloodDrinkerSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private FlavorProfileSystem _flavorProfile = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ReactiveSystem _reaction = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Relays
        SubscribeLocalEvent<BodyComponent, TryDrinkBloodEvent>(_body.RelayEvent);
    }

    /// <summary>
    ///     Executes a blood drinking attempt after the DoAfter finishes.
    /// </summary>
    /// <param name="ent">The drinker.</param>
    [SubscribeLocalEvent]
    private void OnDrinkBloodDoAfter(Entity<BloodDrinkerComponent> ent, ref DrinkBloodDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || args.User == args.Target
            || args.Target == null)
            return;

        TryTransferBlood(ent, args.Target.Value, ent.Comp.TransferAmount, out var ingested);
        FinishIngestion(ent.Owner, args.Target.Value, ingested);
    }

    /// <summary>
    ///     Ingests blood into the stomach of a blood-drinking entity.
    /// </summary>
    /// <param name="ent">The blood drinker's stomach entity.</param>
    [SubscribeLocalEvent]
    private void OnBloodTransferred(Entity<StomachComponent> ent, ref BodyRelayedEvent<TryDrinkBloodEvent> args)
    {
        if (args.Args.Handled)
            return;
        if (!_solutionContainer.ResolveSolution(ent.Owner,
            StomachSystem.DefaultSolutionName,
            ref ent.Comp.Solution))
            return;

        // TODO partial transfers
        _reaction.DoEntityReaction(args.Body, args.Args.Solution, ReactionMethod.Ingestion);
        var success = _solutionContainer.TryAddSolution(ent.Comp.Solution.Value, args.Args.Solution);
        if (success)
            args.Args = args.Args with { Handled = true };
    }

    /// <summary>
    ///     Add blood drinking-related verbs to this entity.
    /// </summary>
    /// <param name="ent">The drinkerrr.</param>
    /// <param name="target">The target entity to get verbs for.</param>
    public void AddBloodDrinkerVerbs(Entity<BloodDrinkerComponent?> ent,
        Entity<BloodstreamComponent?> target,
        ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (target.Owner == args.User || !args.CanInteract || !args.CanAccess)
            return;

        if (!IsInBloodDrinkingRange(ent, target))
            return;

        if (TryGetBloodDrinkerVerb(ent, target, out var verb))
            args.Verbs.Add(verb);
    }

    /// <summary>
    ///     Attempt to retrieve the verb for drinking a target entity's blood.
    /// </summary>
    /// <param name="ent">The drinker.</param>
    /// <param name="target">The target entity to drink from.</param>
    /// <param name="verb">The verb, if successfully retrieved.</param>
    /// <returns>Whether or not this operation was successful.</returns>
    private bool TryGetBloodDrinkerVerb(Entity<BloodDrinkerComponent?> ent,
        Entity<BloodstreamComponent?> target,
        [NotNullWhen(true)] out AlternativeVerb? verb)
    {
        verb = null;

        if (!Resolve(ent.Owner, ref ent.Comp) || !Resolve(target.Owner, ref target.Comp))
            return false;

        verb = new()
        {
            Icon = null,
            Text = Loc.GetString(ent.Comp.VerbLocId),
            Priority = ent.Comp.VerbPriority,
            Act = () => { StartDrinkBlood(ent, target); }
        };

        return true;
    }

    /// <summary>
    ///     Start a DoAfter for this entity to drink a target's blood.
    /// </summary>
    /// <param name="ent">The drinkerrrrrr</param>
    /// <param name="target">The target to drink blood from.</param>
    private void StartDrinkBlood(Entity<BloodDrinkerComponent?> ent, Entity<BloodstreamComponent?> target)
    {
        if (!Resolve(ent.Owner, ref ent.Comp) || !Resolve(target.Owner, ref target.Comp))
            return;

        var ingestTime = ent.Comp.AwakeTargetDrinkTime;
        var ev = new DrinkBloodDoAfterEvent();

        // most of this stuff is just parity with ingestion events
        var doAfterArgs = new DoAfterArgs(EntityManager,
            user: ent,
            delay: ingestTime,
            @event: ev,
            eventTarget: ent,
            target: target)
        {
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnDamage = true,
            MovementThreshold = 0.1f,
            DistanceThreshold = IngestionSystem.MaxFeedDistance,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        DoBiteStartPopups(ent, target);
    }

    /// <summary>
    ///     Logic that runs after a blood drinker finishes drinking from a target.
    /// </summary>
    /// <param name="ent">The drinker.</param>
    /// <param name="target">The target.</param>
    /// <param name="ingested">The solution ingested.</param>
    private void FinishIngestion(Entity<BloodDrinkerComponent?> ent, EntityUid target, Solution? ingested)
    {
        if (!Resolve(ent.Owner, ref ent.Comp) || ingested == null)
            return;

        var didSelfPopup = false;

        if (ProtoMan.TryIndex(ent.Comp.EdibleType, out var edible))
            DoBiteEndPostIngestion(ent, target, ref didSelfPopup, edible, ingested);

        if (ent.Comp.UseBitePopups)
            DoBiteEndPopups(ent, target, ref didSelfPopup);
    }

    /// <summary>
    ///     Attempt to play sound and spawn popup based on the blood-drinking "edible" type.
    /// </summary>
    /// <param name="ent">The drinker.</param>
    /// <param name="target">The target.</param>
    /// <param name="didSelfPopup">Whether or not a popup has been shown to the drinker.</param>
    /// <param name="edible">The edible prototype associated with blood drinking.</param>
    /// <param name="ingested">The solution ingested.</param>
    private void DoBiteEndPostIngestion(Entity<BloodDrinkerComponent?> ent,
        EntityUid target,
        ref bool didSelfPopup,
        EdiblePrototype edible,
        Solution? ingested)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        if (ent.Comp.UseIngestSound)
            _audio.PlayPredicted(edible.UseSound, target, ent);

        // Show a flavor popup to the drinker
        if (ent.Comp.UseTastePopup && !didSelfPopup)
        {
            var flavors = _flavorProfile.GetLocalizedFlavorsMessage(target, ent, ingested);

            // TODO satiated
            _popup.PopupEntity(Loc.GetString(edible.Message,
                ("food", target),
                ("flavors", flavors),
                ("satiated", false)),
                ent,
                ent);

            didSelfPopup = true;
        }
    }

    /// <summary>
    ///     Spawns popups for attempting to drink someone's blood.
    /// </summary>
    /// <param name="ent">The drinker.</param>
    /// <param name="target">The target.</param>
    private void DoBiteStartPopups(Entity<BloodDrinkerComponent?> ent, EntityUid target)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        var userName = Identity.Entity(ent, EntityManager);
        var targetName = Identity.Entity(target, EntityManager);
        var args = new (string, object)[]
        {
            ("user", userName),
            ("target", targetName)
        };

        if (ent.Comp.BitePopupStartSelf != null)
            _popup.PopupEntity(Loc.GetString(ent.Comp.BitePopupStartSelf, args), ent, ent);

        // Popup for target
        if (ent.Comp.BitePopupStartTarget != null)
            _popup.PopupEntity(Loc.GetString(ent.Comp.BitePopupStartTarget, args), ent, target);

        // Popup for everyone else
        if (ent.Comp.BitePopupStartOther != null && ent.Comp.OthersSeeBitePopups)
        {
            var recipients = Filter.Pvs(ent).RemovePlayersByAttachedEntity(ent, target);
            _popup.PopupEntity(Loc.GetString(ent.Comp.BitePopupStartOther, args), ent, recipients, recordReplay: true);
        }
    }

    /// <summary>
    ///     Spawns popups for having finished drinking someone's blood.
    /// </summary>
    /// <param name="ent">The drinker.</param>
    /// <param name="target">The target.</param>
    /// <param name="didSelfPopup">Whether or not a popup has been shown to the drinker.</param>
    private void DoBiteEndPopups(Entity<BloodDrinkerComponent?> ent, EntityUid target, ref bool didSelfPopup)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        var userName = Identity.Entity(ent, EntityManager);
        var targetName = Identity.Entity(target, EntityManager);
        var args = new (string, object)[]
        {
            ("user", userName),
            ("target", targetName)
        };

        // Popup for drinker - overriden by the flavor text popup
        if (ent.Comp.BitePopupEndSelf != null && !didSelfPopup)
        {
            _popup.PopupEntity(Loc.GetString(ent.Comp.BitePopupEndSelf, args), ent, ent);
            didSelfPopup = true;
        }

        // Popup for target
        if (ent.Comp.BitePopupEndTarget != null)
            _popup.PopupEntity(Loc.GetString(ent.Comp.BitePopupEndTarget, args), ent, target);

        // Popup for everyone else
        if (ent.Comp.BitePopupEndOther != null && ent.Comp.OthersSeeBitePopups)
        {
            var recipients = Filter.Pvs(ent).RemovePlayersByAttachedEntity(ent, target);
            _popup.PopupEntity(Loc.GetString(ent.Comp.BitePopupEndOther, args), ent, recipients, recordReplay: true);
        }
    }

    /// <summary>
    ///     Attempt to transfer blood from a target's bloodstream to a blood drinker.
    /// </summary>
    /// <param name="drinker">The drinkerrr.</param>
    /// <param name="target">The target.</param>
    /// <param name="transferAmount">How much blood to transfer.</param>
    /// <returns>Whether or not blood was successfully transferred.</returns>
    private bool TryTransferBlood(EntityUid drinker,
        Entity<BloodstreamComponent?> target,
        FixedPoint2 transferAmount,
        [NotNullWhen(true)] out Solution? ingested)
    {
        ingested = null;

        if (!Resolve(target.Owner, ref target.Comp))
            return false;

        // Make sure target has a valid blood solution
        if (!_solutionContainer.ResolveSolution(target.Owner,
            target.Comp.BloodSolutionName,
            ref target.Comp.BloodSolution))
            return false;

        // Remove blood from target
        ingested = _solutionContainer.SplitSolution(target.Comp.BloodSolution.Value, transferAmount);
        var ev = new TryDrinkBloodEvent(ingested, target);
        RaiseLocalEvent(drinker, ref ev);

        // Nothing handled the blood drinking.
        if (!ev.Handled)
            // Put that shit back
            _solutionContainer.TryAddSolution(target.Comp.BloodSolution.Value, ingested);

        return ev.Handled;
    }

    /// <summary>
    ///     Returns whether or not this entity is in range to drink the blood of the target.
    /// </summary>
    /// <param name="drinker">The drinkerrrr.</param>
    /// <param name="target">The target.</param>
    [PublicAPI]
    public bool IsInBloodDrinkingRange(EntityUid drinker, EntityUid target)
    {
        return _interaction.InRangeUnobstructed(drinker, target);
    }
}

/// <summary>
///     Raised on an entity that is attempting to drink someone's blood.
/// </summary>
/// <param name="Solution">The blood remaining from the target to drink.</param>
/// <param name="Target">The target entity.</param>
/// <param name="Handled">Whether or not a system has processed blood ingestion.</param>
[ByRefEvent]
public record struct TryDrinkBloodEvent(Solution Solution, EntityUid Target, bool Handled = false);

/// <summary>
///     DoAfter event for attempting to drink an entity's blood.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class DrinkBloodDoAfterEvent : SimpleDoAfterEvent;
