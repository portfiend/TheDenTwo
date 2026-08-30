using Content.Shared._DEN.Body.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._DEN.Body.Components;

/// <summary>
///     Applied to entities that have had their blood sipped on by a <see cref="BloodDrinkerComponent"/> entity.
///     This gives the victim examine text to indicate their condition, which can be "concealed" via verb.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
[Access(typeof(SharedBloodDrinkerSystem))]
public sealed partial class BloodDrinkerVictimComponent : Component
{
    /// <summary>
    ///     The examinable text for this entity.
    /// </summary>
    [DataField]
    public LocId ExamineText = "blood-drinker-victim-examine-tooltip";

    /// <summary>
    ///     The localization ID to use for the "conceal" verb.
    /// </summary>
    [DataField("verbName")]
    public LocId VerbLocId = "blood-drinker-victim-conceal-verb";

    /// <summary>
    ///     The localization ID to use for the "conceal" verb's tooltip.
    /// </summary>
    [DataField("verbTooltip")]
    public LocId VerbTooltipLocId = "blood-drinker-victim-conceal-verb-tooltip";

    /// <summary>
    ///     The icon used for the "conceal" verb.
    /// </summary>
    [DataField]
    public SpriteSpecifier VerbIcon = new SpriteSpecifier.Texture(new("/Textures/_DEN/Interface/VerbIcons/blood-plaster.svg.192dpi.png"));

    /// <summary>
    ///     The priority of the "conceal" verb.
    /// </summary>
    [DataField]
    public int VerbPriority = -1;

    /// <summary>
    ///     How long it takes to conceal your own bite marks.
    /// </summary>
    [DataField]
    public TimeSpan ConcealTimeSelf = TimeSpan.FromSeconds(3.0f);

    /// <summary>
    ///     How long it takes for someone else to conceal your bite marks.
    /// </summary>
    [DataField]
    public TimeSpan ConcealTimeOther = TimeSpan.FromSeconds(8.0f);

    /// <summary>
    ///     The sound effect played when the bite marks are concealed.
    /// </summary>
    [DataField]
    public SoundSpecifier? ConcealSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg")
    {
        Params = AudioParams.Default.AddVolume(-3.0f)
    };

    /// <summary>
    ///     The popup text that appears when you begin concealing your own bite marks.
    /// </summary>
    [DataField]
    public LocId SelfConcealPopupStart = "blood-drinker-victim-conceal-self-start-popup";

    /// <summary>
    ///     The popup text that appears when you finish concealing your own bite marks.
    /// </summary>
    [DataField]
    public LocId SelfConcealPopupEnd = "blood-drinker-victim-conceal-self-end-popup";

    /// <summary>
    ///     The popup text that appears when you begin concealing some else's bite marks.
    /// </summary>
    [DataField]
    public LocId PerformerConcealPopupStart = "blood-drinker-victim-conceal-performer-start-popup";

    /// <summary>
    ///     The popup text that appears when you finish concealing some else's bite marks.
    /// </summary>
    [DataField]
    public LocId PerformerConcealPopupEnd = "blood-drinker-victim-conceal-performer-end-popup";

    /// <summary>
    ///     The popup text that appears when someone begins concealing your bite marks.
    /// </summary>
    [DataField]
    public LocId TargetConcealPopupStart = "blood-drinker-victim-conceal-target-start-popup";

    /// <summary>
    ///     The popup text that appears when someone finishes concealing your bite marks.
    /// </summary>
    [DataField]
    public LocId TargetConcealPopupEnd = "blood-drinker-victim-conceal-target-end-popup";
}
