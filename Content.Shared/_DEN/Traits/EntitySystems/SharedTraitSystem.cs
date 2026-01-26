using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._DEN.Traits.Components;
using Robust.Shared.Prototypes;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.EntitySystems;
#pragma warning restore IDE1006 // Naming Styles

public abstract partial class SharedTraitSystem : EntitySystem
{
    private EntityQuery<TraitHolderComponent> _holderQuery;
    private EntityQuery<TraitComponent> _traitQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TraitComponent, ComponentStartup>(OnTraitStartup);
        SubscribeLocalEvent<TraitComponent, ComponentShutdown>(OnTraitShutdown);

        _holderQuery = GetEntityQuery<TraitHolderComponent>();
        _traitQuery = GetEntityQuery<TraitComponent>();
    }

    private void OnTraitStartup(Entity<TraitComponent> ent, ref ComponentStartup arg)
    {
        foreach (var function in ent.Comp.TraitFunctions)
            function.OnTraitAdded(ent.Owner, EntityManager);
    }

    private void OnTraitShutdown(Entity<TraitComponent> ent, ref ComponentShutdown arg)
    {
        // We do this backwards to ensure traits are reversed in the correct order -
        // i.e. if earlier steps are setting up for later steps.
        foreach (var function in ent.Comp.TraitFunctions.Reverse())
            function.OnTraitRemoved(ent.Owner, EntityManager);
    }

    private bool TryGetTraitEntity(EntityUid target,
        EntProtoId traitProto,
        [NotNullWhen(true)] out EntityUid? traitEntity)
    {
        traitEntity = null;

        if (!_holderQuery.TryComp(target, out var holder))
            return false;

        foreach (var trait in holder.Traits?.ContainedEntities ?? [])
        {
            var meta = MetaData(trait);

            if (meta.EntityPrototype is null || meta.EntityPrototype != traitProto)
                continue;

            traitEntity = trait;
            return true;
        }

        return false;
    }

    public bool TryAddTraitEntity(EntityUid target,
        EntProtoId traitProto,
        [NotNullWhen(true)] out EntityUid? traitEntity)
    {
        traitEntity = null;

        EnsureComp<TraitHolderComponent>(target);

        if (!PredictedTrySpawnInContainer(traitProto,
            target,
            TraitHolderComponent.ContainerId,
            out var trait))
            return false;

        if (!_traitQuery.HasComp(trait))
        {
            var traitString = ToPrettyString(trait);
            var targetString = ToPrettyString(target);
            Debug.Fail($"Trait {traitString} was added to {targetString}, but it lacks a TraitComponent!");
            return false;
        }

        traitEntity = trait;
        return true;
    }

    public bool TryRemoveTraitEntity(EntityUid target,
        EntProtoId traitProto)
    {
        if (!TryGetTraitEntity(target, traitProto, out var traitEntity))
            return false;

        PredictedQueueDel(traitEntity);
        return true;
    }
}
