using System.Linq;
using Content.Shared._DEN.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Localizations;

namespace Content.Shared.Body.Systems;

public abstract partial class SharedBloodstreamSystem
{
    [Dependency] private SharedInteractionSystem _interactionSystem = default!;

    private void OnExamined(Entity<BloodstreamComponent> target, ref ExaminedEvent args)
    {
        if (TryComp<BloodExaminerComponent>(args.Examiner, out var bloodExaminer))
            BloodExaminerExamined((args.Examiner, bloodExaminer), target, ref args);
    }

    private void BloodExaminerExamined(Entity<BloodExaminerComponent> examiner,
        Entity<BloodstreamComponent> target,
        ref ExaminedEvent args)
    {
        if (examiner.Owner == target.Owner)
            return;

        // Blood drinker range.
        if (!_interactionSystem.InRangeUnobstructed(examiner.Owner, target.Owner))
            return;

        var bloodSuffix = Loc.GetString(examiner.Comp.BloodSuffix);
        var bloodNames = LocalizeBloodReagentNames(target, bloodSuffix);
        var bloodText = ContentLocalizationManager.FormatList(bloodNames);

        var examineText = Loc.GetString(examiner.Comp.ExamineText, ("target", target), ("blood", bloodText));
        args.PushMarkup(examineText);
    }

    private List<string> LocalizeBloodReagentNames(Entity<BloodstreamComponent> ent, string suffix)
    {
        var reference = ent.Comp.BloodReferenceSolution;
        var names = new List<string>();
        var protos = reference.GetReagentPrototypes(PrototypeManager).Select(p => p.Key);

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
