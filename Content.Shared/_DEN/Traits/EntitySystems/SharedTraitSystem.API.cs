using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._DEN.Traits.Components;
using Content.Shared._DEN.Traits.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.EntitySystems;
#pragma warning restore IDE1006 // Naming Styles

public abstract partial class SharedTraitSystem
{
    /// <summary>
    /// Adds a trait to a given target by entityTrait prototype ID.
    /// </summary>
    /// <param name="target">The entity to receive the trait.</param>
    /// <param name="trait">The trait to add.</param>
    /// <param name="traitEntity">The entity representing the trait.</param>
    /// <returns>If the trait was successfully added to the entity.</returns>
    [PublicAPI]
    public bool TryAddTrait(EntityUid target,
        ProtoId<EntityTraitPrototype> trait,
        [NotNullWhen(true)] out EntityUid? traitEntity)
    {
        traitEntity = null;

        if (!_prototypeManager.TryIndex(trait, out var traitProto) || !CanAddTrait(target, traitProto))
            return false;

        var entity = Spawn();
        var traitComp = EnsureComp<TraitComponent>(entity);
        _serialization.CopyTo(traitProto.TraitFunctions, ref traitComp.TraitFunctions, notNullableOverride: true);
        traitComp.Prototype = trait;

        if (TryAddTraitEntity(target, entity))
            traitEntity = entity;

        return traitEntity != null;
    }

    /// <summary>
    /// Gets the entity associated with a given trait by the trait prototype ID.
    /// </summary>
    /// <param name="target">The entity holding the trait.</param>
    /// <param name="trait">The trait prototype.</param>
    /// <param name="traitEntity">The entity representing the trait.</param>
    /// <returns>Whether or not the trait entity was successfully retrieved.</returns>
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

    /// <summary>
    /// Attempts to delete a trait from an entity by trait prototype ID.
    /// </summary>
    /// <param name="target">The entity whose trait needs to be removed.</param>
    /// <param name="trait">The trait prototype to remove.</param>
    /// <returns>Whether or not the trait was successfully removed.</returns>
    [PublicAPI]
    public bool TryRemoveTrait(EntityUid target, ProtoId<EntityTraitPrototype> trait)
    {
        if (!TryGetTraitEntity(target, trait, out var traitEntity) || Deleted(traitEntity.Value))
            return false;

        PredictedQueueDel(traitEntity);
        return true;
    }

    /// <summary>
    /// Get all the trait prototypes of an entity.
    /// </summary>
    /// <param name="target">The entity to get trait prototypes of.</param>
    /// <param name="traits">The entity trait prototypes this entity has.</param>
    /// <returns>Returns true if the entity has traits and they were successfully retrieved.</returns>
    [PublicAPI]
    public bool TryGetTraits(EntityUid target, [NotNullWhen(true)] out List<ProtoId<EntityTraitPrototype>>? traits)
    {
        traits = null;

        if (!_holderQuery.TryComp(target, out var holder))
            return false;

        var traitEntities = holder.Traits?.ContainedEntities;
        if (traitEntities == null)
            return false;

        traits = new List<ProtoId<EntityTraitPrototype>>();
        foreach (var ent in traitEntities)
        {
            if (_traitQuery.TryComp(ent, out var trait) && trait.Prototype is not null)
                traits.Add(trait.Prototype.Value);
        }

        return true;
    }

    /// <summary>
    /// Checks whether or not this trait prototype can be added to an entity.
    /// </summary>
    /// <param name="target">The entity to add the trait to.</param>
    /// <param name="trait">The trait prototype to use.</param>
    /// <returns>Whether or not the trait can be added.</returns>
    [PublicAPI]
    public bool CanAddTrait(EntityUid target, ProtoId<EntityTraitPrototype> trait)
    {
        if (!_prototypeManager.TryIndex(trait, out var traitProto))
            return false;

        return CanAddTrait(target, traitProto);
    }

    /// <summary>
    /// Checks whether or not this trait prototype can be added to an entity.
    /// </summary>
    /// <param name="target">The entity to add the trait to.</param>
    /// <param name="traitProto">The trait prototype to use.</param>
    /// <returns>Whether or not the trait can be added.</returns>
    [PublicAPI]
    public bool CanAddTrait(EntityUid target, EntityTraitPrototype traitProto)
    {
        if (!_whitelist.CheckBoth(target, blacklist: traitProto.Blacklist, whitelist: traitProto.Whitelist))
            return false;

        return true;
    }
}
