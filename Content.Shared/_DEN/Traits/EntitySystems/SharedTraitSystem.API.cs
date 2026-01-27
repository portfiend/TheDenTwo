using System.Diagnostics.CodeAnalysis;
using Content.Shared._DEN.Traits.Components;
using Content.Shared._DEN.Traits.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.EntitySystems;
#pragma warning restore IDE1006 // Naming Styles

public abstract partial class SharedTraitSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;

    [PublicAPI]
    public bool TryAddTrait(EntityUid target,
        ProtoId<EntityTraitPrototype> trait,
        [NotNullWhen(true)] out EntityUid? traitEntity)
    {
        traitEntity = null;

        if (!_prototypeManager.TryIndex(trait, out var traitProto))
            return false;

        var entity = Spawn();
        var traitComp = EnsureComp<TraitComponent>(entity);
        _serialization.CopyTo(traitProto.TraitFunctions, ref traitComp.TraitFunctions, notNullableOverride: true);
        traitComp.Prototype = trait;

        if (TryAddTraitEntity(target, entity))
            traitEntity = entity;

        return traitEntity != null;
    }

    [PublicAPI]
    public bool TryGetTraitEntity(EntityUid target,
        ProtoId<EntityTraitPrototype> trait,
        [NotNullWhen(true)] out EntityUid? traitEntity)
    {
        traitEntity = null;

        if (!_holderQuery.TryComp(target, out var holder))
            return false;

        foreach (var entity in holder.Traits?.ContainedEntities ?? [])
        {
            if (!_traitQuery.TryComp(entity, out var traitComp)
                || traitComp.Prototype is null
                || traitComp.Prototype.Value != trait)
                continue;

            traitEntity = entity;
            return true;
        }

        return false;
    }

    [PublicAPI]
    public bool TryRemoveTrait(EntityUid target, ProtoId<EntityTraitPrototype> trait)
    {
        if (!TryGetTraitEntity(target, trait, out var traitEntity) || Deleted(traitEntity.Value))
            return false;

        PredictedQueueDel(traitEntity);
        return true;
    }
}
