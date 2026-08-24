using System.Collections.Generic;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.IntegrationTests.Utility;
using Content.Server._DEN.Traits.EntitySystems;
using Content.Shared._DEN.Body.Components;
using Content.Shared._DEN.Traits.Prototypes;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

[TestFixture]
[TestOf(typeof(EntityTraitPrototype))]
public sealed class EntityTraitTests : GameTest
{
    [SidedDependency(Side.Server)] private readonly IngestionSystem _ingestionSystem = default!;
    [SidedDependency(Side.Server)] private readonly TraitSystem _traitSystem = default!;

    private static string[] _species = GameDataScrounger.PrototypesOfKind<SpeciesPrototype>();
    private ProtoId<EntityTraitPrototype> _vampireTraitId = "Vampire";
    private const string VampireFoodTag = "VampireEdible";
    private const string VampireFoodProtoId = "TestPrototypeVampireFood";

    /// <summary>
    ///     A list of example foods that do not contain blood, and thus should not be edible.
    /// </summary>
    private List<EntProtoId> _nonVampireFoods = new()
    {
        "FoodApple",
        "FoodBurgerCheese",
        "FoodBakedCroissantCotton",
        "FoodSoupBungo",
    };

    [TestPrototypes]
    public const string Prototypes =
        @$"
        - type: entity
          id: {VampireFoodProtoId}
          components:
          - type: Item
          - type: Edible
          - type: Tag
            tags:
            - {VampireFoodTag}
          - type: Solution
            id: food
            solution:
              reagents:
              - ReagentId: Nothing
                Quantity: 5
        ";

    [Test]
    [TestOf(typeof(SpeciesPrototype))]
    [TestCaseSource(nameof(_species))]
    [RunOnSide(Side.Server)]
    [Description("Ensure that all species will cleanly add and remove the Vampire trait.")]
    public async Task VampireTraitCleanAddRemove(ProtoId<SpeciesPrototype> speciesId)
    {
        // var species = SProtoMan.Index(speciesId);
        // var body = SEntMan.Spawn(species.Prototype);
        // var entName = SEntMan.ToPrettyString(body);

        // // Skip over species that do not pass the trait requirements.
        // if (!_traitSystem.CanAddTrait(body, _vampireTraitId))
        //     Assert.Ignore($"{entName} does not pass requirements for {_vampireTraitId} trait.");

        // // Get the first stomach of this entity.
        // var getStomach = new GetFirstStomachEvent();
        // SEntMan.EventBus.RaiseLocalEvent(body, ref getStomach);

        // Assert.That(getStomach.Stomach, Is.Not.Null, $"{entName} did not have an available stomach.");
        // var stomach = getStomach.Stomach.Value;

        // // Make a copy of this entity's stomach.
        // SEntMan.TryGetComponent<MetaDataComponent>(stomach.Owner, out var stomachMeta);
        // var stomachId = stomachMeta?.EntityPrototype?.ID;

        // Assert.That(stomachId, Is.Not.Null, $"{entName} lacks a valid stomach prototype!");
        // var compareStomach = SEntMan.Spawn(stomachId);

        // // Validate the stomach's values.
        // SEntMan.TryGetComponent<StomachComponent>(compareStomach, out var copyStomachComp);
        // Assert.That(copyStomachComp, Is.Not.Null,
        //     $"Could not get stomach component for {SEntMan.ToPrettyString(compareStomach)}.");

        // using (Assert.EnterMultipleScope())
        // {
        //     Assert.That(copyStomachComp.SpecialDigestible, Is.EqualTo(stomach.Comp.SpecialDigestible),
        //         $"Stomach for {entName} spawned with unequal {nameof(StomachComponent.SpecialDigestible)} values!");

        //     Assert.That(copyStomachComp.IsSpecialDigestibleExclusive, Is.EqualTo(stomach.Comp.IsSpecialDigestibleExclusive),
        //         $"Stomach for {entName} spawned with unequal {nameof(StomachComponent.IsSpecialDigestibleExclusive)} values!");
        // }

        // // Add the vampire trait to the dummy.
        // var addedSuccessfully = _traitSystem.TryAddTrait(body, _vampireTraitId, out var traitEnt);
        // Assert.That(addedSuccessfully, Is.True,
        //     $"Could not add {_vampireTraitId} trait to {entName}!");

        // // Ensure the mob has the right components.
        // using (Assert.EnterMultipleScope())
        // {
        //     Assert.That(SEntMan.HasComponent<BloodDrinkerComponent>(body), Is.True,
        //         $"{entName} is not a BloodDrinker despite being a vampire!");

        //     // Ensure the mob CANNOT eat non-blood foods.
        //     foreach (var id in _nonVampireFoods)
        //     {
        //         var food = SEntMan.Spawn(id);
        //         var digestible = _ingestionSystem.IsDigestibleBy(food, stomach);

        //         Assert.That(digestible, Is.False,
        //             $"{entName} has non-restrictive diet for {_vampireTraitId} trait on trying to eat {id}!");
        //     }

        //     // Ensure the mob can eat vampire food.
        //     var vampireFood = SEntMan.Spawn(VampireFoodProtoId);
        //     var vampireDigestible = _ingestionSystem.IsDigestibleBy(vampireFood, stomach);

        //     Assert.That(vampireDigestible, Is.True,
        //         $"{entName} could not eat {VampireFoodTag} despite having {_vampireTraitId}!");
        // }

        // // Remove the vampire trait.
        // var removeSuccess = _traitSystem.TryRemoveTrait(body, _vampireTraitId);

        // // Validate trait removal.
        // using (Assert.EnterMultipleScope())
        // {
        //     Assert.That(removeSuccess, Is.True,
        //             $"{entName} could not remove the {_vampireTraitId} trait!");

        //     // Ensure stomach values match pre-trait addition values.
        //     Assert.That(stomach.Comp.SpecialDigestible, Is.EqualTo(copyStomachComp.SpecialDigestible),
        //         $"{SEntMan.ToPrettyString(stomach)} did not reset fields cleanly: {nameof(StomachComponent.SpecialDigestible)}");

        //     Assert.That(stomach.Comp.IsSpecialDigestibleExclusive, Is.EqualTo(copyStomachComp.IsSpecialDigestibleExclusive),
        //         $"{SEntMan.ToPrettyString(stomach)} did not reset fields cleanly: {nameof(StomachComponent.IsSpecialDigestibleExclusive)}");
        // }

    }
}
