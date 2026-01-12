using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowWire.Framework.Core.Helpers;

/// <summary>
/// A fast, deterministic random number generator based on the Xoshiro256** algorithm.
/// Inherits from System.Random for ease of use (Perfect DX).
/// </summary>
public sealed class DeterministicRandom : Random
{
    // Internal state: 4 x 64-bit integers (256 bits total)
    private ulong _s0, _s1, _s2, _s3;

    /// <summary>
    /// Initializes a new instance with a specific seed.
    /// Using the same seed guarantees the same sequence of numbers.
    /// </summary>
    public DeterministicRandom(int seed)
    {
        Reset(seed);
    }

    /// <summary>
    /// Initializes with a time-dependent seed (non-deterministic behavior).
    /// </summary>
    public DeterministicRandom() : this(Environment.TickCount) { }

    /// <summary>
    /// Re-seeds the generator without allocating a new object.
    /// CRITICAL for Object Pooling.
    /// </summary>
    public void Reset(int seed)
    {
        var z = (ulong)seed;

        _s0 = SplitMix64(ref z);
        _s1 = SplitMix64(ref z);
        _s2 = SplitMix64(ref z);
        _s3 = SplitMix64(ref z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong SplitMix64(ref ulong x)
    {
        var z = (x += 0x9e3779b97f4a7c15UL);
        z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9UL;
        z = (z ^ (z >> 27)) * 0x94d049bb133111ebUL;
        return z ^ (z >> 31);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextUInt64()
    {
        // Xoshiro256** Algorithm
        var result = BitOperations.RotateLeft(_s1 * 5, 7) * 9;
        var t = _s1 << 17;

        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;

        _s2 ^= t;
        _s3 = BitOperations.RotateLeft(_s3, 45);

        return result;
    }

    #region Standard System.Random Overrides

    public override int Next()
    {
        // Mask out the sign bit to ensure a non-negative integer
        return (int)(NextUInt64() >> 33);
    }

    public override int Next(int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue);
        return Next(0, maxValue);
    }

    public override int Next(int minValue, int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);

        var range = (long)maxValue - minValue;
        if (range == 0)
        {
            return minValue;
        }

        // Lemire's method for fast, unbiased bounded randomness
        // See: https://arxiv.org/abs/1805.10941
        var rangeUL = (ulong)range;
        var x = NextUInt64();
        var m = (ulong)((UInt128)x * rangeUL >> 64);
        var l = x * rangeUL;

        if (l < rangeUL)
        {
            var t = (ulong)(-(long)rangeUL) % rangeUL;
            while (l < t)
            {
                x = NextUInt64();
                m = (ulong)((UInt128)x * rangeUL >> 64);
                l = x * rangeUL;
            }
        }

        return (int)(m + (ulong)minValue);
    }

    public override double NextDouble()
    {
        return (NextUInt64() >> 11) * (1.0 / (1UL << 53));
    }

    public override void NextBytes(byte[] buffer)
    {
        NextBytes(buffer.AsSpan());
    }

    public override void NextBytes(Span<byte> buffer)
    {
        var i = 0;
        while (i <= buffer.Length - 8)
        {
            Unsafe.WriteUnaligned(ref buffer[i], NextUInt64());
            i += 8;
        }

        if (i < buffer.Length)
        {
            var next = NextUInt64();
            var remaining = buffer.Length - i;
            var remainingBytes = BitConverter.GetBytes(next);
            remainingBytes.AsSpan(0, remaining).CopyTo(buffer[i..]);
        }
    }

    #endregion Standard System.Random Overrides

    #region High Performance Extras

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat()
    {
        return (NextUInt64() >> 40) * (1.0f / (1u << 24));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool NextBool()
    {
        return (NextUInt64() & 1) == 1;
    }

    #endregion High Performance Extras


    #region State Persistence (Critical for Durable Execution)

    /// <summary>
    /// Represents the internal state of the generator.
    /// This is what gets saved to Redis/Database alongside the Workflow State.
    /// </summary>
    public struct RandomState
    {
        public ulong S0 { get; set; }
        public ulong S1 { get; set; }
        public ulong S2 { get; set; }
        public ulong S3 { get; set; }
    }

    /// <summary>
    /// Exports the current state so it can be saved.
    /// </summary>
    public RandomState GetState()
    {
        return new RandomState { S0 = _s0, S1 = _s1, S2 = _s2, S3 = _s3 };
    }

    /// <summary>
    /// Resumes the generator from a saved state.
    /// </summary>
    public void LoadState(RandomState state)
    {
        // Guard against zero-state
        if (state.S0 == 0 && state.S1 == 0 && state.S2 == 0 && state.S3 == 0)
        {
            var defaultSeed = 2300;
            var temp = new DeterministicRandom(defaultSeed);
            _s0 = temp._s0; _s1 = temp._s1; _s2 = temp._s2; _s3 = temp._s3;
            return;
        }

        _s0 = state.S0;
        _s1 = state.S1;
        _s2 = state.S2;
        _s3 = state.S3;
    }

    #endregion State Persistence (Critical for Durable Execution)
}