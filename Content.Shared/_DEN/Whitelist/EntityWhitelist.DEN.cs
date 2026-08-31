using System.Linq;

namespace Content.Shared.Whitelist;

public sealed partial class EntityWhitelist
{
    // Value-based equality operator
    public override bool Equals(object? obj)
    {
        if (obj is not EntityWhitelist other)
            return false;

        if (RequireAll != other.RequireAll)
            return false;

        if (!CompareEnumerable(Components, other.Components))
            return false;

        if (!CompareEnumerable(Tags, other.Tags))
            return false;

        if (!CompareEnumerable(Sizes, other.Sizes))
            return false;

        return true;
    }

    private static bool CompareEnumerable<T>(IEnumerable<T>? left, IEnumerable<T>? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        // Order doesn't matter.
        return left.ToHashSet().SetEquals(right);
    }

    public override int GetHashCode()
    {
        // least expensive hashcode function
        var hash = new HashCode();
        hash.Add(RequireAll);

        foreach (var comp in Components ?? [])
            hash.Add(comp);

        foreach (var tag in Tags ?? new())
            hash.Add(tag);

        foreach (var size in Sizes ?? new())
            hash.Add(size);

        return hash.ToHashCode();
    }
}
