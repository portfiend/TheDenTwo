using Content.Shared._DEN.Traits.EntitySystems;
using Content.Shared._DEN.Traits.Prototypes;
using Content.Shared.GameTicking;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Roles;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Server._DEN.Traits.EntitySystems;
#pragma warning restore IDE1006 // Naming Styles

public sealed partial class TraitSystem : SharedTraitSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (args.JobId == null
            || !_prototypeManager.Resolve<JobPrototype>(args.JobId, out var protoJob)
            || !protoJob.ApplyTraits)
            return;

        var mob = args.Mob;

        foreach (var traitId in args.Profile.EntityTraitPreferences)
        {
            if (!_prototypeManager.TryIndex(traitId, out var trait))
            {
                Log.Error($"No trait found with ID {traitId}!");
                continue;
            }

            if (!trait.Selectable)
            {
                Log.Error($"Tried to spawn unselectable trait {traitId} on {ToPrettyString(mob)}!");
                continue;
            }

            if (trait.AllowedSpecies != null)
            {
                if (!TryComp<HumanoidProfileComponent>(mob, out var profile)
                    || !trait.AllowedSpecies.Contains(profile.Species))
                {
                    Log.Error($"Tried to spawn trait {traitId} on {ToPrettyString(mob)} with invalid species: {profile?.Species ?? "null"}!");
                    continue;
                }
            }

            TryAddTrait(mob, traitId, out _);
        }
    }
}
