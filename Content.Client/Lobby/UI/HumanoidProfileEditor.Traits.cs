using System.Linq;
using Content.Client.Lobby.UI.Roles;
using Content.Client.Stylesheets;
using Content.Shared.Traits;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{

    /// <summary>
    /// Refreshes traits selector
    /// </summary>
    public void RefreshTraits()
    {
        TabContainer.SetTabTitle(3, Loc.GetString("humanoid-profile-editor-traits-tab"));

        // DEN: Nuke this entire function. We using entity traits
        RefreshEntityTraits();
    }
}
