using System.Linq;
using JetBrains.Annotations;

namespace Content.Shared._DEN.Requirements.PlayerRequirements;

/// <summary>
///     An abstract class for requirements to determine how many items in a set should be selected.
/// </summary>
/// <remarks>
///     For example: A player selecting multiple traits that overlap with a certain required group of traits.
/// </remarks>
[ImplicitDataDefinitionForInheritors]
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class CountRequirement
{
    /// <summary>
    ///     Gets a string reason representation for this range.
    /// </summary>
    /// <remarks>
    ///     This would slot into the sentence "You must have [reason] of the following items: [items]".
    /// </remarks>
    /// <example>"At least 1", "between 2 and 5", "all"</example>
    /// <returns>A string representation of this range's requirement bounds.</returns>
    public abstract string GetReason();

    /// <summary>
    ///     Check if our currently-selected items in a collection meets this
    ///     requirement, against a collection of required items.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="have">The items we have selected.</param>
    /// <param name="required">The items required for this condition.</param>
    /// <returns>Whether or not we fulfill this requirement.</returns>
    public abstract bool CheckRequirement<T>(IEnumerable<T> have, IEnumerable<T> required);

    /// <summary>
    ///     Get how many of the required items in a collection we currently have.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="have">The items we have selected.</param>
    /// <param name="required">The items required for this condition.</param>
    /// <returns>How many of the required items we have.</returns>
    protected static int GetFulfilledCount<T>(IEnumerable<T> have, IEnumerable<T> required)
    {
        return have.Intersect(required).Count();
    }
}

/// <summary>
///     To fulfill this requirement, you must have exactly some number of items.
/// </summary>
public sealed partial class ConstantCountRequirement : CountRequirement
{
    /// <summary>
    ///     How many items in the collection you need to pass the requirement.
    /// </summary>
    [DataField]
    public int Count;

    /// <inheritdoc />
    public override string GetReason()
    {
        return Loc.GetString("count-requirement-constant-reason",
            ("count", Count));
    }

    /// <inheritdoc />
    public override bool CheckRequirement<T>(IEnumerable<T> have, IEnumerable<T> required)
    {
        var count = GetFulfilledCount(have, required);
        return count == Count;
    }
}

/// <summary>
///     To fulfill this requirement, you must have a number of items between two values, or either a minimum / maximum.
/// </summary>
public sealed partial class RangeCountRequirement : CountRequirement
{
    /// <summary>
    ///     Minimum amount of required items you need.
    /// </summary>
    [DataField]
    public int? Min = null;

    /// <summary>
    ///     Maximum amount of required items you can have.
    /// </summary>
    [DataField]
    public int? Max = null;

    /// <inheritdoc />
    public override string GetReason()
    {
        return (Min, Max) switch
        {
            (not null, not null) => Loc.GetString("count-requirement-range-minmax-reason",
                ("minimum", Min),
                ("maximum", Max)),

            (null, not null) => Loc.GetString("count-requirement-range-maximum-reason",
                ("maximum", Max)),

            (not null, null) => Loc.GetString("count-requirement-range-minimum-reason",
                ("minimum", Min)),

            _ => string.Empty
        };
    }

    /// <inheritdoc />
    public override bool CheckRequirement<T>(IEnumerable<T> have, IEnumerable<T> required)
    {
        var count = GetFulfilledCount(have, required);
        return (Min == null || count >= Min)
            && (Max == null || count <= Max);
    }
}

/// <summary>
///     To fulfill the requirement, you must have all items in the required collection.
/// </summary>
public sealed partial class AllCountRequirement : CountRequirement
{
    /// <inheritdoc />
    public override string GetReason()
    {
        return Loc.GetString("count-requirement-all-reason");
    }

    /// <inheritdoc />
    public override bool CheckRequirement<T>(IEnumerable<T> have, IEnumerable<T> required)
    {
        var count = GetFulfilledCount(have, required);
        return count == required.Count();
    }
}
