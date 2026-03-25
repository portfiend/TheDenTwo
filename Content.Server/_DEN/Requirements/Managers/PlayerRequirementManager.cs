using Content.Server.GameTicking;
using Content.Shared._DEN.Requirements.Managers;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Shared.Player;

namespace Content.Server._DEN.Requirements.Managers;

/// <inheritdoc />
public sealed partial class PlayerRequirementManager : SharedPlayerRequirementManager
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ISharedPlaytimeManager _playtimeManager = default!;

    /// <inheritdoc />
    public override PlayerRequirementContext GetPlayerContext(ICommonSession session)
    {
        var playtimes = _playtimeManager.GetPlayTimes(session);
        var profile = _gameTicker.GetPlayerProfile(session);

        return new()
        {
            Playtimes = playtimes,
            Profile = profile,
        };
    }
}
