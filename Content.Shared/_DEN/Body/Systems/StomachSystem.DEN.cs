using System.Linq;
using Content.Shared._DEN.Traits.TraitFunctions;
using Content.Shared.Body.Components;
using Content.Shared.Whitelist;

namespace Content.Shared.Body.Systems;

public sealed partial class StomachSystem
{
    [Dependency] private BodySystem _body = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, AddTraitFilterSpecialDigestibleEvent>(_body.RelayEvent);
        SubscribeLocalEvent<BodyComponent, AddTraitSpecialDigestibleEvent>(_body.RelayEvent);
        SubscribeLocalEvent<BodyComponent, GetFirstStomachEvent>(_body.RelayEvent);
        SubscribeLocalEvent<BodyComponent, RemoveTraitSpecialDigestibleEvent>(_body.RelayEvent);
        SubscribeLocalEvent<BodyComponent, ResetTraitSpecialDigestibleEvent>(_body.RelayEvent);
    }

    /// <summary>
    ///     Adds a special digestible addition trait.
    /// </summary>
    /// <param name="ent">The stomach entity.</param>
    [SubscribeLocalEvent]
    private void OnAddTraitSpecialDigestible(Entity<StomachComponent> ent, ref BodyRelayedEvent<AddTraitSpecialDigestibleEvent> args)
    {
        if (args.Args.Handled || _whitelist.IsWhitelistFail(args.Args.OrganWhitelist, ent))
            return;

        if (args.Args.SpecialDigestible != null)
        {
            var oldDigestible = ent.Comp.SpecialDigestible;
            var newDigestible = MergeSpecialDigestible(ent, args.Args.SpecialDigestible);

            ent.Comp.SpecialDigestible = newDigestible;

            // Store the old whitelist value.
            args.Args = args.Args with { SpecialDigestible = oldDigestible };
        }

        if (args.Args.IsSpecialDigestibleExclusive != null)
        {
            var wasExclusive = ent.Comp.IsSpecialDigestibleExclusive;
            ent.Comp.IsSpecialDigestibleExclusive = args.Args.IsSpecialDigestibleExclusive.Value;

            // Store the old exclusive value.
            args.Args = args.Args with { IsSpecialDigestibleExclusive = wasExclusive };
        }

        Dirty(ent);
        args.Args = args.Args with { Handled = true };
    }

    /// <summary>
    ///     Removes a special digestible addition trait.
    /// </summary>
    /// <param name="ent">The stomach entity.</param>
    [SubscribeLocalEvent]
    private void OnRemoveTraitSpecialDigestible(Entity<StomachComponent> ent, ref BodyRelayedEvent<RemoveTraitSpecialDigestibleEvent> args)
    {
        if (args.Args.Handled || _whitelist.IsWhitelistFail(args.Args.OrganWhitelist, ent))
            return;

        if (args.Args.SpecialDigestible != null)
            ent.Comp.SpecialDigestible = args.Args.SpecialDigestible;

        if (args.Args.WasSpecialDigestibleExclusive != null)
            ent.Comp.IsSpecialDigestibleExclusive = args.Args.WasSpecialDigestibleExclusive.Value;

        Dirty(ent);
        args.Args = args.Args with { Handled = true };
    }

    [SubscribeLocalEvent]
    private void OnAddTraitFilterSpecialDigestible(Entity<StomachComponent> ent,
        ref BodyRelayedEvent<AddTraitFilterSpecialDigestibleEvent> args)
    {
        if (args.Args.Handled
            || _whitelist.IsWhitelistFail(args.Args.OrganWhitelist, ent)
            || args.Args.SpecialDigestible == null)
            return;

        var oldDigestible = ent.Comp.SpecialDigestible;

        ent.Comp.SpecialDigestible = FilterSpecialDigestible(ent, args.Args.SpecialDigestible);
        Dirty(ent);

        args.Args = args.Args with
        {
            Handled = true,
            SpecialDigestible = oldDigestible
        };
    }

    [SubscribeLocalEvent]
    private void OnResetTraitSpecialDigestible(Entity<StomachComponent> ent,
        ref BodyRelayedEvent<ResetTraitSpecialDigestibleEvent> args)
    {
        if (args.Args.Handled || _whitelist.IsWhitelistFail(args.Args.OrganWhitelist, ent))
            return;

        ent.Comp.SpecialDigestible = args.Args.SpecialDigestible;
        Dirty(ent);

        args.Args = args.Args with { Handled = true };
    }

    /// <summary>
    ///     Get the first stomach of an entity.
    /// </summary>
    [SubscribeLocalEvent]
    private void OnGetFirstStomach(Entity<StomachComponent> ent, ref BodyRelayedEvent<GetFirstStomachEvent> args)
    {
        if (args.Args.Stomach != null)
            return;

        args.Args = args.Args with { Stomach = ent };
    }

    private EntityWhitelist? FilterSpecialDigestible(Entity<StomachComponent> ent, EntityWhitelist newDigestible)
    {
        var oldDigestible = ent.Comp.SpecialDigestible;
        if (oldDigestible == null)
            return oldDigestible;

        var newComps = FilterFields(oldDigestible.Components, newDigestible.Components);
        var newSizes = FilterFields(oldDigestible.Sizes, newDigestible.Sizes);
        var newTags = FilterFields(oldDigestible.Tags, newDigestible.Tags);
        var requireAll = oldDigestible.RequireAll;

        // this is yucky but whatever
        var filteredDigestible = new EntityWhitelist()
        {
            Components = newComps?.ToArray(),
            Sizes = newSizes?.ToList(),
            Tags = newTags?.ToList(),
            RequireAll = requireAll
        };

        return filteredDigestible;
    }

    private EntityWhitelist MergeSpecialDigestible(Entity<StomachComponent> ent, EntityWhitelist newDigestible)
    {
        var oldDigestible = ent.Comp.SpecialDigestible;
        if (oldDigestible == null)
            return newDigestible;

        var newComps = MergeFields(oldDigestible.Components, newDigestible.Components);
        var newSizes = MergeFields(oldDigestible.Sizes, newDigestible.Sizes);
        var newTags = MergeFields(oldDigestible.Tags, newDigestible.Tags);
        var requireAll = oldDigestible.RequireAll || newDigestible.RequireAll;

        // this is yucky but whatever
        var mergedDigestible = new EntityWhitelist()
        {
            Components = newComps?.ToArray(),
            Sizes = newSizes?.ToList(),
            Tags = newTags?.ToList(),
            RequireAll = requireAll
        };

        return mergedDigestible;
    }

    private static IEnumerable<T>? MergeFields<T>(IEnumerable<T>? oldValue, IEnumerable<T>? newValue)
    {
        if (oldValue == null && newValue == null)
            return null;

        if (newValue == null)
            return oldValue;

        if (oldValue == null)
            return newValue;

        return oldValue.Union(newValue);
    }

    private static IEnumerable<T>? FilterFields<T>(IEnumerable<T>? oldValue, IEnumerable<T>? newValue)
    {
        if (oldValue == null || newValue == null)
            return oldValue;

        return oldValue.Intersect(newValue);
    }
}

/// <summary>
///     An event that adds special digestible types to stomachs that pass the whitelist.
/// </summary>
/// <param name="OrganWhitelist">The whitelist for organs to add metabolizers to.</param>
[ByRefEvent]
public record struct AddTraitSpecialDigestibleEvent(
    EntityWhitelist? SpecialDigestible,
    EntityWhitelist? OrganWhitelist,
    bool? IsSpecialDigestibleExclusive,
    bool Handled = false);

/// <summary>
///     An event that resets special digestible types from stomachs that pass the whitelist.
/// </summary>
/// <param name="OrganWhitelist">The whitelist for organs to add metabolizers to.</param>
[ByRefEvent]
public record struct RemoveTraitSpecialDigestibleEvent(
    EntityWhitelist? SpecialDigestible,
    EntityWhitelist? OrganWhitelist,
    bool? WasSpecialDigestibleExclusive,
    bool Handled = false);

/// <summary>
///     An event that modifies special digestion to intersect a certain value for stomachs that pass a whitelist.
/// </summary>
/// <param name="OrganWhitelist">The whitelist for organs to add metabolizers to.</param>
[ByRefEvent]
public record struct AddTraitFilterSpecialDigestibleEvent(
    EntityWhitelist? SpecialDigestible,
    EntityWhitelist? OrganWhitelist,
    bool Handled = false);

/// <summary>
///     An event that resets the special digestion list of stomachs that pass the whitelist..
/// </summary>
/// <param name="OrganWhitelist">The whitelist for organs to add metabolizers to.</param>
[ByRefEvent]
public record struct ResetTraitSpecialDigestibleEvent(
    EntityWhitelist? SpecialDigestible,
    EntityWhitelist? OrganWhitelist,
    bool Handled = false);

/// <summary>
///     Helper event to get the first available stomach of an entity.
/// </summary>
[ByRefEvent]
public record struct GetFirstStomachEvent(Entity<StomachComponent>? Stomach = null);
