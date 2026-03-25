using System.Linq;
using Content.Shared._DEN.Requirements.Managers;
using Content.Shared._DEN.Traits.Prototypes;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Requirements.PlayerRequirements;

/// <summary>
///     Checks if a player's character has a required number of the given traits.
/// </summary>
public sealed partial class PlayerTraitRequirement : PlayerRequirement
{
    /// <summary>
    ///     Traits that the character needs to have to the pass the requirement.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<EntityTraitPrototype>> Traits = new();

    [DataField]
    public CountRequirement Count;

    /// <inheritdoc/>
    public override bool PreCheck(PlayerRequirementContext context)
    {
        return context.Profile != null;
    }

    /// <inheritdoc/>
    public override bool CheckRequirement(PlayerRequirementContext context)
    {
        if (context.Profile == null)
            return false;

        var profileTraits = context.Profile.EntityTraitPreferences;
        return Count.CheckRequirement(profileTraits, Traits);
    }

    /// <inheritdoc/>
    public override string? GetReason()
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var traitNames = Traits.Select(t => LocalizeTrait(t, protoMan));
        var traitList = string.Join(", ", traitNames);
        var constraintReason = Count.GetReason();

        return Loc.GetString("player-requirement-trait-reason",
            ("inverted", Inverted),
            ("constraint", constraintReason),
            ("traits", traitList));
    }

    /// <summary>
    ///     Localizes a trait ID into a formatted trait name.
    /// </summary>
    /// <param name="traitId">The ID of the trait.</param>
    /// <param name="protoMan">The prototype manager.</param>
    /// <returns>A formatted trait name string.</returns>
    private static string LocalizeTrait(ProtoId<EntityTraitPrototype> traitId, IPrototypeManager protoMan)
    {
        var traitName = traitId;

        if (protoMan.TryIndex(traitId, out var trait))
            traitName = Loc.GetString(trait.Name);

        return Loc.GetString("player-requirement-format-trait", ("trait", traitName));
    }
}

/// <summary>
///     Checks if a player's character is one of the following species.
/// </summary>
public sealed partial class PlayerSpeciesRequirement : PlayerRequirement
{
    /// <summary>
    ///     Character's profile must be one of these species to pass the requirement.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>> Species = new();

    /// <inheritdoc/>
    public override bool PreCheck(PlayerRequirementContext context)
    {
        return context.Profile != null;
    }

    /// <inheritdoc/>
    public override bool CheckRequirement(PlayerRequirementContext context)
    {
        if (context.Profile == null)
            return false;

        return Species.Contains(context.Profile.Species);
    }

    /// <inheritdoc/>
    public override string? GetReason()
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        // Add all species names to a list
        var speciesList = new List<string>();
        foreach (var speciesId in Species)
            speciesList.Add(LocalizeSpecies(speciesId, protoMan));

        var joinedSpecies = string.Join(", ", speciesList);

        // "Must be one of these species: Human, Dwarf, Slimeperson"
        return Loc.GetString("player-requirement-species-reason",
            ("inverted", Inverted),
            ("species", joinedSpecies));
    }

    /// <summary>
    ///     Localizes a species ID into a formatted species name.
    /// </summary>
    /// <param name="speciesId">The ID of the species.</param>
    /// <param name="protoMan">The prototype manager.</param>
    /// <returns>A formatted species name string.</returns>
    private static string LocalizeSpecies(ProtoId<SpeciesPrototype> speciesId, IPrototypeManager protoMan)
    {
        var speciesName = speciesId;
        if (!protoMan.TryIndex(speciesId, out var species))
            return speciesName;

        speciesName = Loc.GetString(species.Name);
        return Loc.GetString("player-requirement-format-species", ("species", speciesName));
    }
}
