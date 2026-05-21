using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DEN.Movement.Components;

/// <summary>
/// Changes footstep sounds ONLY when this entity is not wearing shoes.
/// </summary>
/// <remarks>
/// This is similar to FootstepModifierComponent.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BarestepModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public SoundSpecifier? FootstepSoundCollection;
}
