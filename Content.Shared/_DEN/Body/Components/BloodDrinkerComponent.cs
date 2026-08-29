using Content.Shared._DEN.Body.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Nutrition.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DEN.Body.Components;

/// <summary>
///     Applied to entities that are capable of drinking the blood of other entities via verb.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
[Access(typeof(SharedBloodDrinkerSystem))]
public sealed partial class BloodDrinkerComponent : Component
{
    /// <summary>
    ///     Whether or not the target must be incapacitated.
    /// </summary>
    [DataField]
    public bool MustBeIncapacitated = false;

    /// <summary>
    ///     How long it takes from an awake target.
    /// </summary>
    [DataField]
    public TimeSpan AwakeTargetDrinkTime = TimeSpan.FromSeconds(8.0f);

    /// <summary>
    ///     How long it takes to drink from an incapacitated target.
    /// </summary>
    [DataField]
    public TimeSpan IncapacitatedTargetDrinkTime = TimeSpan.FromSeconds(3.0f);

    /// <summary>
    ///     How much blood this entity drinks per sip.
    /// </summary>
    [DataField]
    public FixedPoint2 TransferAmount = FixedPoint2.New(10.0f);

    /// <summary>
    ///     The localization ID to use for the verb.
    /// </summary>
    [DataField("verbName")]
    public LocId VerbLocId = "blood-drinker-bite-verb";

    /// <summary>
    ///     The icon used for the "bite" verb.
    /// </summary>
    [DataField]
    public SpriteSpecifier VerbIcon = new SpriteSpecifier.Texture(new("/Textures/_DEN/Interface/VerbIcons/vampire-bite.svg.192dpi.png"));

    /// <summary>
    ///     The priority of the ingestion verb.
    /// </summary>
    [DataField]
    public int VerbPriority = 2;
    /// <summary>
    ///     An edible type associated with the blood feeding action.
    /// </summary>
    /// <remarks>
    ///     This is used to determine ingestion sounds, popup text, and more.
    /// </remarks>
    [DataField]
    public ProtoId<EdiblePrototype> EdibleType = "Drink";

    /// <summary>
    ///     Whether or not the drinker and target will get popups for bite attempts.
    /// </summary>
    [DataField]
    public bool UseBitePopups = true;

    /// <summary>
    ///     Whether or not other entities (besides the drinker and target) can see bite popups.
    /// </summary>
    [DataField]
    public bool OthersSeeBitePopups = true;

    /// <summary>
    ///     Whether or not this entity will make a sound on ingestion.
    /// </summary>
    [DataField]
    public bool UseIngestSound = true;

    /// <summary>
    ///     Whether or not this entity will get a taste popup for the blood they ingest.
    /// </summary>
    [DataField]
    public bool UseTastePopup = true;

    /// <summary>
    ///     Locale ID for the bite attempt popup that shows up to the drinker.
    /// </summary>
    /// <remarks>
    ///     Takes a "user" and "target" parameter.
    /// </remarks>
    [DataField]
    public LocId? BitePopupStartSelf = "blood-drinker-popup-start-self";

    /// <summary>
    ///     Locale ID for the bite attempt popup that shows up to other players.
    /// </summary>
    /// <remarks>
    ///     Takes a "user" and "target" parameter.
    /// </remarks>
    [DataField]
    public LocId? BitePopupStartOther = "blood-drinker-popup-start-other";

    /// <summary>
    ///     Locale ID for the bite attempt popup that shows up to the target.
    /// </summary>
    /// <remarks>
    ///     Takes a "user" and "target" parameter.
    /// </remarks>
    [DataField]
    public LocId? BitePopupStartTarget = "blood-drinker-popup-start-target";

    /// <summary>
    ///     Locale ID for the bite finished popup that shows up to the drinker.
    /// </summary>
    /// <remarks>
    ///     Takes a "user" and "target" parameter.
    /// </remarks>
    [DataField]
    public LocId? BitePopupEndSelf = "blood-drinker-popup-end-self";

    /// <summary>
    ///     Locale ID for the bite finished popup that shows up to other players.
    /// </summary>
    /// <remarks>
    ///     Takes a "user" and "target" parameter.
    /// </remarks>
    [DataField]
    public LocId? BitePopupEndOther = "blood-drinker-popup-end-other";

    /// <summary>
    ///     Locale ID for the bite finished popup that shows up to the target.
    /// </summary>
    /// <remarks>
    ///     Takes a "user" and "target" parameter.
    /// </remarks>
    [DataField]
    public LocId? BitePopupEndTarget = "blood-drinker-popup-end-target";
}
