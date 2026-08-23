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

        var traitGroups = GetTraitCategories(traits, out var uncategorized)
            .OrderBy(p => _prototypeManager.Index<TraitCategoryPrototype>(p.Key).Priority);

        // Uncategorized comes first.
        if (uncategorized.Count > 0)
            AddCategoryBox(null, uncategorized);

        // Then, everything else.
        foreach (var (categoryId, traitProtos) in traitGroups)
            AddCategoryBox(categoryId, traitProtos);
    }

    private void AddCategoryBox(string? categoryId, List<EntityTraitPrototype> traitProtos)
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

    private TraitCategoryBox GetTraitCategoryBox(string? categoryId, List<EntityTraitPrototype> traitProtos)
    {
        var categoryBox = new TraitCategoryBox(_prototypeManager);
        categoryBox.SetProfile(Profile);
        categoryBox.SetTraits(traitProtos);

        if (categoryId != null && _prototypeManager.TryIndex<TraitCategoryPrototype>(categoryId, out var category))
            categoryBox.SetCategory(category);

        return categoryBox;
    }

    private Dictionary<string, List<EntityTraitPrototype>> GetTraitCategories(
        List<EntityTraitPrototype> traits,
        out List<EntityTraitPrototype> uncategorized)
    {
        var traitGroups = new Dictionary<string, List<EntityTraitPrototype>>();
        uncategorized = new List<EntityTraitPrototype>();

        foreach (var trait in traits)
        {
            if (trait.Category == null)
            {
                uncategorized.Add(trait);
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
