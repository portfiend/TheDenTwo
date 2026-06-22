
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using JetBrains.Annotations;

namespace Content.Shared._DEN.Body.EntitySystems;

/// <summary>
///     A system that holds APIs and logic related to giving entities the ability to drink blood.
/// </summary>
public abstract partial class SharedBloodDrinkerSystem : EntitySystem
{
    [Dependency] private BodySystem _body = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private ReactiveSystem _reaction = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Relays
        SubscribeLocalEvent<BodyComponent, TryDrinkBloodEvent>(_body.RelayEvent);

        // Subscriptions
        SubscribeLocalEvent<StomachComponent, BodyRelayedEvent<TryDrinkBloodEvent>>(OnBloodDrank);
    }

    private void OnBloodDrank(Entity<StomachComponent> ent, ref BodyRelayedEvent<TryDrinkBloodEvent> args)
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
