using Content.Shared.Hands.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.TraitFunctions;
#pragma warning restore IDE1006 // Naming Styles

[UsedImplicitly]
public sealed partial class SpawnWithGearTrait : ITraitFunction
{
    [DataField(required: true)] public List<EntProtoId> Gear = new();
    [DataField] public bool DeleteGearWhenRemoved = false;

    [ViewVariables] public List<EntityUid>? AddedGear = null;

    public void OnTraitAdded(EntityUid owner, EntityManager entityManager)
    {
        AddedGear = new();

        var hands = entityManager.System<SharedHandsSystem>();
        var transform = entityManager.TransformQuery.GetComponent(owner);
        var coords = transform.Coordinates;

        foreach (var entProto in Gear)
        {
            var entity = entityManager.SpawnAtPosition(entProto, coordinates: coords);
            hands.TryPickupAnyHand(owner,
                entity,
                checkActionBlocker: false);

            AddedGear.Add(entity);
        }
    }

    public void OnTraitRemoved(EntityUid owner, EntityManager entityManager)
    {
        if (AddedGear is null || !DeleteGearWhenRemoved)
            return;

        foreach (var gear in AddedGear)
            if (!entityManager.Deleted(gear))
                entityManager.PredictedQueueDeleteEntity(gear);
    }
}
