using Content.Shared.FixedPoint;
using Content.Shared.Nutrition.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Body.Components;

/// <summary>
///     Applied to entities that are capable of drinking the blood of other entities via verb.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
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
    public TimeSpan AwakeTargetDrinkTime = TimeSpan.FromSeconds(6.0f);

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
    ///     The priority of the ingestion verb.
    /// </summary>
    [DataField]
    public int VerbPriority = 2;

    /// <summary>
    ///     Whether or not the drinker and target will get popups for bite attempts.
    /// </summary>
    [DataField]
    public bool UseBitePopups = true;

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
    ///     An edible type associated with the blood feeding action.
    /// </summary>
    /// <remarks>
    ///     This is used to determine ingestion sounds, popup text, and more.
    /// </remarks>
    [DataField]
    public ProtoId<EdiblePrototype> EdibleType = "Drink";
}
