using Content.Shared._DEN.Requirements.Managers;

namespace Content.Shared._DEN.Requirements.PlayerRequirements;

/// <summary>
///     This interface is used to check some parameters associated with a player (a
///     <see cref="PlayerRequirementContext"/>) to determine whether or not the player is allowed to
///     use a certain feature.
/// </summary>
/// <remarks>
///     This is a replacement of the old JobRequirement system.
/// </remarks>
[ImplicitDataDefinitionForInheritors]
public partial interface IPlayerRequirement
{
    /// <summary>
    ///     Whether or not this requirement is inverted.
    ///     In all cases where this requirement should fail, it will succeed instead, and vice versa.
    /// </summary>
    [DataField]
    bool Inverted { get; set; }

    /// <summary>
    ///     Requirements undergo a "PreCheck" to check if the context value has all relevant parameters.
    ///     If this is false, then the requirement auto-passes the requirement if the pre-check fails.
    /// </summary>
    /// <remarks>
    ///     Say you have a requirement that checks your playtimes. In the pre-check, we check if the
    ///     context's playtimes are non-null. If your playtimes are null, we fail the pre-check.
    ///
    ///     If MustPassPreCheck is false, then this will treat it as auto-passing the requirement;
    ///     we ignore the requirement entirely. If MustPassPreCheck is true, we auto-fail the
    ///     requirement instead.
    /// </remarks>
    [DataField]
    bool MustPassPreCheck { get; set; }

    /// <summary>
    ///     Check if the given context has all parameters required in order to perform an actual check.
    /// </summary>
    /// <remarks>
    ///     Because context parameters are meant to be optional, we can use this information to ignore
    ///     (auto-succeed) checks where we lack all the needded parameters, depending on the value of
    ///     <see cref="MustPassPreCheck"/>.
    /// </remarks>
    /// <param name="context">A definition of parameters to check against the requirement.</param>
    /// <returns></returns>
    bool PreCheck(PlayerRequirementContext context);

    /// <summary>
    ///     Check a context's fields against this requirement to determine if it passes or fails.
    /// </summary>
    /// <param name="context">A definition of parameters to check against the requirement.</param>
    /// <returns>Whether or not the given context passes this requirement.</returns>
    bool CheckRequirement(PlayerRequirementContext context);

    /// <summary>
    ///     Get a localized string representing how this requirement displays to players.
    /// </summary>
    /// <remarks>
    ///     This should display differently depending on the value of <see cref="Inverted"/>.
    /// </remarks>
    /// <returns>An optional string representing the requirement text to display to a player.</returns>
    string? GetReason();
}

/// <summary>
///     Abstract class inherited by other player requirements.
/// </summary>
public abstract partial class PlayerRequirement : IPlayerRequirement
{
    /// <inheritdoc/>
    [DataField] public bool Inverted { get; set; } = false;

    /// <inheritdoc/>
    [DataField] public bool MustPassPreCheck { get; set; } = false;

    /// <inheritdoc/>
    public abstract bool CheckRequirement(PlayerRequirementContext context);

    /// <inheritdoc/>
    public abstract string? GetReason();

    /// <inheritdoc/>
    public abstract bool PreCheck(PlayerRequirementContext context);
}
