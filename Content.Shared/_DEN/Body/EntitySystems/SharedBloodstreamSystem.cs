using System.Linq;
using Content.Shared._DEN.Body.Components;
using Content.Shared._DEN.Body.EntitySystems;
using Content.Shared._DEN.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Examine;
using Content.Shared.Localizations;
using Content.Shared.Verbs;

namespace Content.Shared.Body.Systems;

public abstract partial class SharedBloodstreamSystem
{
    [Dependency] private SharedBloodDrinkerSystem _bloodDrinker = default!;

    [SubscribeLocalEvent]
    private void OnGetVerbs(Entity<BloodstreamComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;

        if (TryComp<BloodDrinkerComponent>(user, out var drinker))
            _bloodDrinker.AddBloodDrinkerVerbs((user, drinker), ent.AsNullable(), ref args);
    }

    [SubscribeLocalEvent]
    private void OnExamined(Entity<BloodstreamComponent> target, ref ExaminedEvent args)
    {
        if (TryComp<BloodExaminerComponent>(args.Examiner, out var bloodExaminer))
            ExamineBlood((args.Examiner, bloodExaminer), target, ref args);
    }

    /// <summary>
    ///     Adds examine text to the tooltip of this entity for bloodstream contents.
    /// </summary>
    /// <param name="examiner">The entity examining the bloodstream.</param>
    /// <param name="target">The target with a bloodstream.</param>
    private void ExamineBlood(Entity<BloodExaminerComponent> examiner,
        Entity<BloodstreamComponent> target,
        ref ExaminedEvent args)
    {
        // Can't examine your own. Noseblindness or something
        if (examiner.Owner == target.Owner)
            return;

        // Smelling range :)
        if (!_bloodDrinker.IsInBloodDrinkingRange(examiner.Owner, target.Owner))
            return;

        var bloodSuffix = Loc.GetString(examiner.Comp.BloodSuffix);
        var bloodNames = LocalizeBloodReagentNames(target, bloodSuffix);
        var bloodText = ContentLocalizationManager.FormatList(bloodNames); // "A, B, and C blood"

        var examineText = Loc.GetString(examiner.Comp.ExamineText, ("target", target), ("blood", bloodText));
        args.PushMarkup(examineText);
    }

    /// <summary>
    ///     Gets a list of localized names for reagents composing this entity's bloodstream.
    /// </summary>
    /// <param name="ent">The entity with a bloodstream.</param>
    /// <param name="suffix">The word at the end of a "sensible" blood type - e.g. "blood" for "insect blood".</param>
    /// <returns>A list of localized names for bloodstream reagents.</returns>
    private List<string> LocalizeBloodReagentNames(Entity<BloodstreamComponent> ent, string suffix)
    {
        var reference = ent.Comp.BloodReferenceSolution;
        var names = new List<string>();
        var protos = reference.GetReagentPrototypes(ProtoMan).Select(p => p.Key);

        foreach (var blood in protos)
        {
            var bloodName = blood.LocalizedName;

            // Blood reagent text is colored.
            var bloodText = Loc.GetString("blood-examiner-component-chemical",
                ("color", blood.SubstanceColor.ToHexNoAlpha()),
                ("blood", bloodName));

            // Add "blood" to the end if it doesn't already have it, to make the sentence make sense.
            // E.g. "You can smell her apple juice." -> "You can smell her apple juice blood."
            if (protos.Last() == blood && !bloodName.EndsWith(suffix))
                bloodText = Loc.GetString("blood-examiner-component-examine-not-blood",
                    ("chemical", bloodText),
                    ("suffix", suffix));

            names.Add(bloodText);
        }

        return names;
    }
}
