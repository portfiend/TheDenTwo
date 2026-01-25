#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.TraitFunctions;
#pragma warning restore IDE1006 // Naming Styles

public interface ITraitFunction
{
    void OnTraitAdded(EntityUid owner, EntityManager entityManager);
    void OnTraitRemoved(EntityUid owner, EntityManager entityManager);
}
