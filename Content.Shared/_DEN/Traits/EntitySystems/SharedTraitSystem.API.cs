using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Content.Shared._DEN.Traits.Components;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.EntitySystems;
#pragma warning restore IDE1006 // Naming Styles

public abstract partial class SharedTraitSystem
{
    [PublicAPI]
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

    [PublicAPI]
    public bool TryRemoveTraitEntity(EntityUid target,
        EntProtoId traitProto)
    {
        if (!TryGetTraitEntity(target, traitProto, out var traitEntity))
            return false;

        PredictedQueueDel(traitEntity);
        return true;
    }
}
