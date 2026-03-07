using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings
{
    [Prototype]
    public sealed partial class MarkingPrototype : IPrototype, IInheritingPrototype // DEN: Make inheriting
    {
        [IdDataField]
        public string ID { get; private set; } = "uwu";

        // DEN start: Make markings inheriting
        [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<MarkingPrototype>))]
        public string[]? Parents { get; private set; }

        [NeverPushInheritance]
        [AbstractDataField]
        public bool Abstract { get; private set; }
        // End DEN

        public string Name { get; private set; } = default!;

        [DataField("bodyPart", required: true)]
        public HumanoidVisualLayers BodyPart { get; private set; } = default!;

        [DataField]
        [AlwaysPushInheritance] // DEN
        public List<ProtoId<MarkingsGroupPrototype>>? GroupWhitelist;

        [DataField("sexRestriction")]
        public Sex? SexRestriction { get; private set; }

        [DataField("forcedColoring")]
        public bool ForcedColoring { get; private set; } = false;

        [DataField("coloring")]
        public MarkingColors Coloring { get; private set; } = new();

        /// <summary>
        /// Do we need to apply any displacement maps to this marking? Set to false if your marking is incompatible
        /// with a standard human doll, and is used for some special races with unusual shapes
        /// </summary>
        [DataField]
        public bool CanBeDisplaced { get; private set; } = true;

        [DataField("sprites", required: true)]
        public List<SpriteSpecifier> Sprites { get; private set; } = default!;

        // DEN start: categorization of markings

        /// <summary>
        ///     A list of "categories" that this marking belongs to.
        /// </summary>
        /// <remarks>
        ///     This will eventually be used for in-round marking customization - such as interactions that
        ///     can change your scars, tattoos, gauze wraps, or underwear in the middle of a round.
        /// </remarks>
        [DataField]
        public HashSet<string> Categories = new();

        // DEN end

        public Marking AsMarking()
        {
            return new Marking(ID, Sprites.Count);
        }
    }
}
