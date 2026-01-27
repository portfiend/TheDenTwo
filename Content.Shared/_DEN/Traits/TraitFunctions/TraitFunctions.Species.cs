using Content.Shared._DEN.Species.Components;
using Content.Shared._DEN.Species.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.TraitFunctions;
#pragma warning restore IDE1006 // Naming Styles

[UsedImplicitly]
public sealed partial class ModifyPhysiologyDescriptorTrait : ITraitFunction
{
    [DataField] public LocId BaseText = string.Empty;
    [DataField] public LocId? PrefixText = string.Empty;

    [ViewVariables] public LocId OldBaseText = string.Empty;
    [ViewVariables] public LocId? OldPrefixText = string.Empty;

    public void OnTraitAdded(EntityUid owner, EntityManager entityManager)
    {
        var physiologySystem = entityManager.System<SharedPhysiologyDescriptionSystem>();

        if (!entityManager.TryGetComponent<PhysiologyDescriptionComponent>(owner, out var descriptor))
            return;

        var entity = (owner, descriptor);

        if (BaseText != string.Empty)
        {
            OldBaseText = descriptor.BaseLabel;
            physiologySystem.SetBaseText(entity, BaseText);
        }

        if (PrefixText != string.Empty)
        {
            OldPrefixText = descriptor.PrefixLabel;
            physiologySystem.SetPrefixText(entity, PrefixText);
        }
    }

    public void OnTraitRemoved(EntityUid owner, EntityManager entityManager)
    {
        var physiologySystem = entityManager.System<SharedPhysiologyDescriptionSystem>();

        if (!entityManager.TryGetComponent<PhysiologyDescriptionComponent>(owner, out var descriptor))
            return;

        var entity = (owner, descriptor);

        if (OldBaseText != string.Empty)
            physiologySystem.SetBaseText(entity, OldBaseText);

        if (OldPrefixText != string.Empty)
            physiologySystem.SetPrefixText(entity, OldPrefixText);
    }
}
