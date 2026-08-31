using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Metabolism;
using Content.Shared.Whitelist;
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

/// <summary>
///     A trait that adds a metabolizer to this entity's organs.
/// </summary>
[UsedImplicitly]
public sealed partial class AddMetabolizerTrait : ITraitFunction
{
    /// <summary>
    ///     A set of metabolizers to add to this entity's organs.
    /// </summary>
    [DataField] public HashSet<ProtoId<MetabolizerTypePrototype>> MetabolizerTypes = [];

    /// <summary>
    ///     A whitelist of allowed organs to use for this trait.
    /// </summary>
    /// <remarks>
    ///     For example: you can have this only apply to the heart or lungs by whitelisting Heart or Lung components.
    /// </remarks>
    [DataField] public EntityWhitelist? OrganWhitelist = null;

    public void OnTraitAdded(EntityUid owner, EntityManager entityManager)
    {
        var ev = new AddTraitMetabolizerEvent(MetabolizerTypes, OrganWhitelist);
        entityManager.EventBus.RaiseLocalEvent(owner, ref ev);
    }

    public void OnTraitRemoved(EntityUid owner, EntityManager entityManager)
    {
        var ev = new RemoveTraitMetabolizerEvent(MetabolizerTypes, OrganWhitelist);
        entityManager.EventBus.RaiseLocalEvent(owner, ref ev);
    }
}

/// <summary>
///     A trait that adds special digestion types to an entity's organs.
/// </summary>
[UsedImplicitly]
public sealed partial class AddSpecialDigestibleTrait : ITraitFunction
{
    /// <summary>
    ///     Allows you to set the value of special digestibility.
    /// </summary>
    [DataField] public bool? IsSpecialDigestibleExclusive = null;

    /// <summary>
    ///     Special digestion fields to add to this entity.
    /// </summary>
    [DataField] public EntityWhitelist? SpecialDigestible = null;

    /// <summary>
    ///     A whitelist of allowed organs to use for this trait.
    /// </summary>
    [DataField] public EntityWhitelist? OrganWhitelist = null;

    /// <summary>
    ///     Special digestion fields to add to this entity.
    /// </summary>
    // TODO: This doesn't handle multi-stomach well.
    [DataField] public EntityWhitelist? PreviousSpecialDigestible = null;

    /// <summary>
    ///     The value of <see cref="StomachComponent.IsSpecialDigestibleExclusive"/> before this trait was processed.
    /// </summary>
    // TODO: This doesn't handle multi-stomach well.
    [ViewVariables] public bool? WasSpecialDigestibleExclusive = null;

    public void OnTraitAdded(EntityUid owner, EntityManager entityManager)
    {
        var ev = new AddTraitSpecialDigestibleEvent(SpecialDigestible, OrganWhitelist, IsSpecialDigestibleExclusive);
        entityManager.EventBus.RaiseLocalEvent(owner, ref ev);

        if (!ev.Handled)
            return;

        // These get replaced during the event to represent the "previous" values.
        PreviousSpecialDigestible = ev.OldSpecialDigestible;
        WasSpecialDigestibleExclusive = ev.WasSpecialDigestibleExclusive;
    }

    public void OnTraitRemoved(EntityUid owner, EntityManager entityManager)
    {
        var ev = new RemoveTraitSpecialDigestibleEvent(PreviousSpecialDigestible, OrganWhitelist, WasSpecialDigestibleExclusive);
        entityManager.EventBus.RaiseLocalEvent(owner, ref ev);
    }
}

/// <summary>
///     A trait that filters the SpecialDigestible parameters of an entity's stomach to a certain whitelist.
/// </summary>
/// <remarks>
///     For example: one could filter the digestible tag list to just Pill and Crayon, and have mobs with
///     unrelated specialized diets (cloth, fruit, meat) filter their extra tags out.
/// </remarks>
[UsedImplicitly]
public sealed partial class FilterSpecialDigestibleTrait : ITraitFunction
{
    /// <summary>
    ///     Special digestion fields to add to this entity.
    /// </summary>
    [DataField(required: true)] public EntityWhitelist SpecialDigestible;

    /// <summary>
    ///     A whitelist of allowed organs to use for this trait.
    /// </summary>
    [DataField] public EntityWhitelist? OrganWhitelist = null;

    /// <summary>
    ///     Special digestion fields to add to this entity.
    /// </summary>
    // TODO: This doesn't handle multi-stomach well.
    [DataField] public EntityWhitelist? PreviousSpecialDigestible = null;

    public void OnTraitAdded(EntityUid owner, EntityManager entityManager)
    {
        var ev = new AddTraitFilterSpecialDigestibleEvent(SpecialDigestible, OrganWhitelist);
        entityManager.EventBus.RaiseLocalEvent(owner, ref ev);

        if (!ev.Handled)
            return;

        // These get replaced during the event to represent the "previous" values.
        PreviousSpecialDigestible = ev.OldSpecialDigestible;
    }

    public void OnTraitRemoved(EntityUid owner, EntityManager entityManager)
    {
        var ev = new ResetTraitSpecialDigestibleEvent(PreviousSpecialDigestible, OrganWhitelist);
        entityManager.EventBus.RaiseLocalEvent(owner, ref ev);
    }
}
