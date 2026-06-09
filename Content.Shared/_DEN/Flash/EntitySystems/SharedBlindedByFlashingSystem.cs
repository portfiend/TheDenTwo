
using Content.Shared._DEN.Flash.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Flash;
using Robust.Shared.Random;

namespace Content.Shared._DEN.Flash.EntitySystems;

public sealed partial class SharedBlindedByFlashingSystem : EntitySystem
{
    [Dependency] private BlindableSystem _blindable = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlindedByFlashingComponent, AfterFlashedEvent>(OnAfterFlashed);
    }

    /// <summary>
    ///     Add eye damage to this entity when they are flashed.
    /// </summary>
    /// <param name="ent">The entity that receives eye damage when flashed.</param>
    public void OnAfterFlashed(Entity<BlindedByFlashingComponent> ent, ref AfterFlashedEvent args)
    {
        if (ent.Owner != args.Target
            || ent.Comp.Damage == 0
            || ent.Comp.Chance <= 0.0f)
            return;

        if (ent.Comp.Chance < 1.0f
            && _random.NextFloat() > ent.Comp.Chance)
            return;

        _blindable.AdjustEyeDamage(ent.Owner, ent.Comp.Damage);
    }
}
