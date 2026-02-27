using Content.Shared._DEN.Kitchen.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared.Kitchen
{
    /// <summary>
    ///    A recipe for space microwaves.
    /// </summary>
    [Prototype("microwaveMealRecipe")]
    public sealed partial class FoodRecipePrototype : IPrototype, IInheritingPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        // DEN start: Make microwave recipes inheriting
        [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<FoodRecipePrototype>))]
        public string[]? Parents { get; private set; }

        [NeverPushInheritance]
        [AbstractDataField]
        public bool Abstract { get; private set; }
        // DEN end

        [DataField("name")]
        private string _name = string.Empty;

        [DataField]
        public string Group = "Other";

        [DataField("reagents", customTypeSerializer:typeof(PrototypeIdDictionarySerializer<FixedPoint2, ReagentPrototype>))]
        private Dictionary<string, FixedPoint2> _ingsReagents = new();

        [DataField("solids", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<FixedPoint2, EntityPrototype>))]
        private Dictionary<string, FixedPoint2> _ingsSolids = new ();

        [DataField("result", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string Result { get; private set; } = string.Empty;

        [DataField("time")]
        public uint CookTime { get; private set; } = 5;

        public string Name => Loc.GetString(_name);

        // TODO Turn this into a ReagentQuantity[]
        public IReadOnlyDictionary<string, FixedPoint2> IngredientsReagents => _ingsReagents;
        public IReadOnlyDictionary<string, FixedPoint2> IngredientsSolids => _ingsSolids;

        /// <summary>
        /// Is this recipe unavailable in normal circumstances?
        /// </summary>
        [DataField]
        public bool SecretRecipe = false;

        // DEN start: Appliance coooking

        /// <summary>
        ///     A set of valid appliances that can produce this recipe.
        /// </summary>
        /// <remarks>
        ///     For example, an appliance might be only capable of producing recipes with the "Microwave" type.
        /// </remarks>
        [DataField(required: true)]
        public HashSet<ProtoId<CookingAppliancePrototype>> RecipeTypes = new();

        // End DEN

        /// <summary>
        ///    Count the number of ingredients in a recipe for sorting the recipe list.
        ///    This makes sure that where ingredient lists overlap, the more complex
        ///    recipe is picked first.
        /// </summary>
        public FixedPoint2 IngredientCount()
        {
            FixedPoint2 n = 0;
            n += _ingsReagents.Count; // number of distinct reagents
            foreach (FixedPoint2 i in _ingsSolids.Values) // sum the number of solid ingredients
            {
                n += i;
            }
            return n;
        }
    }
}
