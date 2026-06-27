using System.Linq;
using Content.Shared.Body;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared.Metabolism;

public sealed partial class MetabolizerSystem
{
    [Dependency] private BodySystem _body = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    private void InitializeDen()
    {
        SubscribeLocalEvent<BodyComponent, AddTraitMetabolizerEvent>(_body.RelayEvent);
        SubscribeLocalEvent<BodyComponent, RemoveTraitMetabolizerEvent>(_body.RelayEvent);

        SubscribeLocalEvent<MetabolizerComponent, BodyRelayedEvent<AddTraitMetabolizerEvent>>(OnAddTraitMetabolizerEvent);
        SubscribeLocalEvent<MetabolizerComponent, BodyRelayedEvent<RemoveTraitMetabolizerEvent>>(OnRemoveTraitMetabolizerEvent);
    }

    /// <summary>
    ///     Adds metabolizers to metabolizer organs that pass a whitelist.
    /// </summary>
    /// <param name="ent">The metabolizer organ.</param>
    private void OnAddTraitMetabolizerEvent(Entity<MetabolizerComponent> ent,
        ref BodyRelayedEvent<AddTraitMetabolizerEvent> args)
    {
        if (args.Args.Handled
            || _whitelist.IsWhitelistFail(args.Args.OrganWhitelist, ent)
            || args.Args.Metabolizers.Count == 0)
            return;

        var metabolizers = args.Args.Metabolizers;

        if (ent.Comp.MetabolizerTypes != null)
            metabolizers = metabolizers.Union(ent.Comp.MetabolizerTypes).ToHashSet();

        ent.Comp.MetabolizerTypes = metabolizers;
        args.Args = args.Args with
        {
            Handled = true,
        };
    }

    /// <summary>
    ///     Removes metabolizers from metabolizer organs that pass a whitelist.
    /// </summary>
    /// <param name="ent">The metabolizer organ.</param>
    private void OnRemoveTraitMetabolizerEvent(Entity<MetabolizerComponent> ent,
        ref BodyRelayedEvent<RemoveTraitMetabolizerEvent> args)
    {
        if (args.Args.Handled
            || _whitelist.IsWhitelistFail(args.Args.OrganWhitelist, ent)
            || args.Args.Metabolizers.Count == 0
            || ent.Comp.MetabolizerTypes == null)
            return;

        // TODO: add smarter handling for this
        var metabolizers = ent.Comp.MetabolizerTypes
            .Except(args.Args.Metabolizers)
            .ToHashSet();

        ent.Comp.MetabolizerTypes = metabolizers;
        args.Args = args.Args with
        {
            Handled = true,
        };
    }
}

/// <summary>
///     An event that adds metabolizers to subscribed entities that pass the whitelist.
/// </summary>
/// <param name="Metabolizers">The metabolizers to add.</param>
/// <param name="OrganWhitelist">The whitelist for organs to add metabolizers to.</param>
/// <param name="Handled">Whether or not this event has been handled.</param>
[ByRefEvent]
public record struct AddTraitMetabolizerEvent(HashSet<ProtoId<MetabolizerTypePrototype>> Metabolizers,
    EntityWhitelist? OrganWhitelist = null,
    bool Handled = false);

/// <summary>
///     An event that removes metabolizers from subscribed entities that pass the whitelist.
/// </summary>
/// <param name="Metabolizers">The metabolizers to add.</param>
/// <param name="OrganWhitelist">The whitelist for organs to add metabolizers to.</param>
/// <param name="Handled">Whether or not this event has been handled.</param>
[ByRefEvent]
public record struct RemoveTraitMetabolizerEvent(HashSet<ProtoId<MetabolizerTypePrototype>> Metabolizers,
    EntityWhitelist? OrganWhitelist = null,
    bool Handled = false);
