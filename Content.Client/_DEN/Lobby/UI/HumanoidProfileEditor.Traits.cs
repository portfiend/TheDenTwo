using Content.Client._DEN.Lobby.UI.Traits;
using Content.Shared._DEN.Traits.Prototypes;
using Content.Shared.Traits;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private void RefreshEntityTraits()
    {
        TraitsList.RemoveAllChildren();

        var traits = _prototypeManager.EnumeratePrototypes<EntityTraitPrototype>()
            .OrderBy(t => Loc.GetString(t.Name))
            .ToList();

        if (traits.Count == 0)
        {
            AddEmptyTraitsLabel();
            return;
        }

        var traitGroups = GetTraitCategories(traits)
            .OrderBy(p => p.Key == TraitCategoryPrototype.Default ? 0
                : _prototypeManager.Index<TraitCategoryPrototype>(p.Key).Priority);

        foreach (var (categoryId, traitProtos) in traitGroups)
        {
            var categoryBox = GetTraitCategoryBox(categoryId, traitProtos);
            categoryBox.OnPreferenceUpdated += profile =>
            {
                Profile = profile;
                SetDirty();
                RefreshTraits();
            };

            TraitsList.AddChild(categoryBox);
        }
    }

    private TraitCategoryBox GetTraitCategoryBox(string categoryId, List<EntityTraitPrototype> traitProtos)
    {
        var category = categoryId == TraitCategoryPrototype.Default
            ? null
            : _prototypeManager.Index<TraitCategoryPrototype>(categoryId);

        var categoryBox = new TraitCategoryBox(_prototypeManager);
        categoryBox.SetProfile(Profile);
        categoryBox.SetTraits(traitProtos);

        if (category is not null)
            categoryBox.SetCategory(category);

        return categoryBox;
    }

    private Dictionary<string, List<EntityTraitPrototype>> GetTraitCategories(List<EntityTraitPrototype> traits)
    {
        var traitGroups = new Dictionary<string, List<EntityTraitPrototype>>();
        var defaultTraits = new List<EntityTraitPrototype>();
        traitGroups.Add(TraitCategoryPrototype.Default, defaultTraits);

        foreach (var trait in traits)
        {
            if (trait.Category == null)
            {
                defaultTraits.Add(trait);
                continue;
            }

            if (!_prototypeManager.HasIndex(trait.Category))
                continue;

            var group = traitGroups.GetOrNew(trait.Category);
            group.Add(trait);
        }

        return traitGroups;
    }

    private void AddEmptyTraitsLabel()
    {
        var label = new Label
        {
            Text = Loc.GetString("humanoid-profile-editor-no-traits"),
            FontColorOverride = Color.Gray,
        };

        TraitsList.AddChild(label);
    }
}
