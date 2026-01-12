using System.Collections;
using System.Collections.Immutable;

namespace FlowWire.Framework.Analyzers.Generators;

/// <summary>
/// A wrapper for ImmutableArray that implements value equality for correct incremental caching.
/// </summary>
readonly internal struct EquatableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableArray<T>>, IEnumerable<T>
{
    private readonly ImmutableArray<T> _array = array;

    public bool Equals(EquatableArray<T> other)
    {
        return _array.SequenceEqual(other._array);
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = 17;
        foreach (var item in _array)
        {
            hash = hash * 31 + (item?.GetHashCode() ?? 0);
        }
        return hash;
    }

    public IEnumerator<T> GetEnumerator()
    {
        IEnumerable<T> collection = _array.IsDefault ? [] : _array;
        return collection.GetEnumerator();
    }

    public T this[int index] => _array[index];

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
