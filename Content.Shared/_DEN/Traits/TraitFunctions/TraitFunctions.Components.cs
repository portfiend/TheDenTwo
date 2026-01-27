using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.TraitFunctions;
#pragma warning restore IDE1006 // Naming Styles

[UsedImplicitly]
public sealed partial class AddComponentTrait : ITraitFunction
{
    /// <summary>
    /// Component definitions to add to the entity.
    /// </summary>
    [DataField(required: true)] public ComponentRegistry Components { get; private set; } = [];

    /// <summary>
    /// Whether or not the components on the entity should be replaced entirely upon adding this trait.
    /// </summary>
    /// <remarks>
    /// This can be destructive if the trait is later removed.
    /// </remarks>
    [DataField] public bool RemoveExisting = false;

    [ViewVariables] public List<IComponent>? AddedComponents = null;

    public void OnTraitAdded(EntityUid owner, EntityManager entityManager)
    {
        AddedComponents = new();
        var componentFactory = IoCManager.Resolve<IComponentFactory>();
        var serialization = IoCManager.Resolve<ISerializationManager>();

        foreach (var (name, entry) in Components)
        {
            var reg = componentFactory.GetRegistration(name);

            if (entityManager.HasComponent(owner, reg) && !RemoveExisting)
                continue;

            var comp = componentFactory.GetComponent(reg);
            serialization.CopyTo(entry.Component, ref comp, notNullableOverride: true);
            entityManager.AddComponent(owner, comp, overwrite: RemoveExisting); // TODO: Check for success...?
            AddedComponents.Add(comp);
        }
    }

    public void OnTraitRemoved(EntityUid owner, EntityManager entityManager)
    {
        if (AddedComponents is null)
            return;

        foreach (var comp in AddedComponents)
        {
            if (comp.Deleted)
                continue;

            entityManager.RemoveComponent(owner, comp);
        }
    }
}
