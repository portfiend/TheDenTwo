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
    ///     How long it takes to execute the "conceal" verb.
    /// </summary>
    [DataField]
    public TimeSpan ConcealTime = TimeSpan.FromSeconds(3.0f);

    /// <summary>
    ///     The popup text that appears when you start the "conceal" verb.
    /// </summary>
    [DataField]
    public LocId ConcealPopupStart = "blood-drinker-victim-conceal-start-popup";

    /// <summary>
    ///     The popup text that appears when you finish the "conceal" verb.
    /// </summary>
    [DataField]
    public LocId ConcealPopupEnd = "blood-drinker-victim-conceal-end-popup";

    /// <summary>
    ///     The sound effect played when you conceal your bite marks.
    /// </summary>
    [DataField]
    public SoundSpecifier? ConcealSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg")
    {
        Params = AudioParams.Default.AddVolume(-3.0f)
    };
}
