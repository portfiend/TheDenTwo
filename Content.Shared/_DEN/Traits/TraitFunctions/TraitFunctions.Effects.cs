using Content.Shared._DEN.Species.Components;
using Content.Shared._DEN.Species.EntitySystems;
using Content.Shared.StatusEffectNew;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.TraitFunctions;
#pragma warning restore IDE1006 // Naming Styles

[UsedImplicitly]
public sealed partial class ApplyStatusEffectTrait : ITraitFunction
{
    [DataField(required: true)] public HashSet<EntProtoId> StatusEffects { get; private set; } = new();
    [DataField] public bool DeleteEffectsWhenRemoved = true;

    [ViewVariables] public List<EntProtoId>? AddedEffects = null;

    public void OnTraitAdded(EntityUid owner, EntityManager entityManager)
    {
        AddedEffects = new();
        var statusEffects = entityManager.System<StatusEffectsSystem>();

        foreach (var effect in StatusEffects)
            if (statusEffects.TrySetStatusEffectDuration(owner, effect))
                AddedEffects.Add(effect);
    }

    public void OnTraitRemoved(EntityUid owner, EntityManager entityManager)
    {
        if (AddedEffects is null || !DeleteEffectsWhenRemoved)
            return;

        var statusEffects = entityManager.System<StatusEffectsSystem>();
        foreach (var effect in AddedEffects)
            statusEffects.TryRemoveStatusEffect(owner, effect);
    }
}
