using System.Diagnostics.CodeAnalysis;
using Content.Shared._DEN.Traits.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.EntitySystems;
#pragma warning restore IDE1006 // Naming Styles

public abstract partial class SharedTraitSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [PublicAPI]
    public bool TryAddTrait(EntityUid target,
        ProtoId<EntityTraitPrototype> trait,
        [NotNullWhen(true)] out EntityUid? traitEntity)
    {
        traitEntity = null;

        if (!_prototypeManager.TryIndex(trait, out var traitProto)
            || traitProto.Entity == null)
            return false;

        if (TryAddTraitEntity(target, traitProto.Entity.Value, out var entity))
            traitEntity = entity;

        return traitEntity != null;
    }

    [PublicAPI]
    public bool TryGetTraitEntity(EntityUid target,
        ProtoId<EntityTraitPrototype> trait,
        [NotNullWhen(true)] out EntityUid? traitEntity)
    {
        traitEntity = null;

        if (!_prototypeManager.TryIndex(trait, out var traitProto)
            || traitProto.Entity == null)
            return false;

        if (TryGetTraitEntity(target, traitProto.Entity.Value, out var entity))
            traitEntity = entity;

        return traitEntity != null;
    }

    [PublicAPI]
    public bool TryRemoveTrait(EntityUid target, ProtoId<EntityTraitPrototype> trait)
    {
        if (!_prototypeManager.TryIndex(trait, out var traitProto)
            || traitProto.Entity == null)
            return false;

        return TryRemoveTraitEntity(target, traitProto.Entity.Value);
    }
}
