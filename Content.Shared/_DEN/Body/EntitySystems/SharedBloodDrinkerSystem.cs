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
using Content.Shared.Interaction;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Shared.Serialization;

namespace Content.Shared._DEN.Body.EntitySystems;

/// <summary>
///     A system that holds APIs and logic related to giving entities the ability to drink blood.
/// </summary>
public abstract partial class SharedBloodDrinkerSystem : EntitySystem
{
    [Dependency] private BodySystem _body = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private ReactiveSystem _reaction = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Relays
        SubscribeLocalEvent<BodyComponent, TryDrinkBloodEvent>(_body.RelayEvent);

        // Subscriptions
        SubscribeLocalEvent<BloodstreamComponent, GetVerbsEvent<AlternativeVerb>>(OnGetBloodstreamVerbs);
        SubscribeLocalEvent<StomachComponent, BodyRelayedEvent<TryDrinkBloodEvent>>(OnBloodTransferred);
    }

    private void OnGetBloodstreamVerbs(Entity<BloodstreamComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;

        if (ent.Owner == user || !args.CanInteract || !args.CanAccess)
            return;

        if (TryComp<BloodDrinkerComponent>(user, out var drinker))
            AddBloodDrinkerVerbs((user, drinker), ent.AsNullable(), ref args);
    }

    private void AddBloodDrinkerVerbs(Entity<BloodDrinkerComponent?> ent,
        Entity<BloodstreamComponent?> target,
        ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!IsInBloodDrinkingRange(ent, target))
            return;

        if (TryGetBloodDrinkerVerb(ent, target, out var verb))
            args.Verbs.Add(verb);
    }

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
    }

    /// <summary>
    ///     Ingests blood into the stomach of a blood-drinking entity.
    /// </summary>
    /// <param name="ent">The blood drinker's stomach entity.</param>
    private void OnBloodTransferred(Entity<StomachComponent> ent, ref BodyRelayedEvent<TryDrinkBloodEvent> args)
    {
        if (args.Args.Handled)
            return;

        if (!_solutionContainer.ResolveSolution(ent.Owner,
            StomachSystem.DefaultSolutionName,
            ref ent.Comp.Solution))
            return;

        _reaction.DoEntityReaction(args.Body, args.Args.Solution, ReactionMethod.Ingestion);
        _solutionContainer.TryAddSolution(ent.Comp.Solution.Value, args.Args.Solution);
    }

    /// <summary>
    ///     Attempt to transfer blood from a target's bloodstream to a blood drinker.
    /// </summary>
    /// <param name="drinker">The drinkerrr.</param>
    /// <param name="target">The target.</param>
    /// <param name="transferAmount">How much blood to transfer.</param>
    /// <returns>Whether or not blood was successfully transferred.</returns>
    private bool TryTransferBlood(EntityUid drinker, Entity<BloodstreamComponent?> target, FixedPoint2 transferAmount)
    {
        if (!Resolve(target.Owner, ref target.Comp))
            return false;

        // Make sure target has a valid blood solution
        if (!_solutionContainer.ResolveSolution(target.Owner,
            target.Comp.BloodSolutionName,
            ref target.Comp.BloodSolution))
            return false;

        // Remove blood from target
        var ingested = _solutionContainer.SplitSolution(target.Comp.BloodSolution.Value, transferAmount);
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
/// <param name="Solution">The blood removed from the target.</param>
/// <param name="Target">The target entity.</param>
/// <param name="Handled">Whether or not a system has processed blood ingestion.</param>
[ByRefEvent]
public record struct TryDrinkBloodEvent(Solution Solution, EntityUid Target, bool Handled = false);

/// <summary>
///     DoAfter event for attempting to drink an entity's blood.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class DrinkBloodDoAfterEvent : SimpleDoAfterEvent;
