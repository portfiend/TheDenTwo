namespace Content.Shared._DEN.Flash.Components;

/// <summary>
///     Entities with this component will take eye damage when they are flashed.
/// </summary>
[RegisterComponent]
public sealed partial class BlindedByFlashingComponent : Component
{
    /// <summary>
    ///     How much eye damage this entity receives when flashed.
    /// </summary>
    [DataField]
    public int Damage = 1;

    /// <summary>
    ///     The chance of this entity receiving eye damage when flashed.
    /// </summary>
    [DataField]
    public float Chance = 1.0f;
}
