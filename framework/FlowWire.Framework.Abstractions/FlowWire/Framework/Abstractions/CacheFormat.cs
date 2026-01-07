using System;
using System.Collections.Generic;
using System.Text;

namespace FlowWire.Framework.Abstractions;

public enum CacheFormat
{
    /// <summary>
    /// (Default) Raw binary serialization.
    /// Fastest CPU performance. Ideal for small DTOs (< 1KB).
    /// </summary>
    MemoryPack = 0,

    /// <summary>
    /// Binary serialization with Brotli compression.
    /// Saves Redis memory/bandwidth but costs CPU. Ideal for large DTOs (> 1KB).
    /// </summary>
    MemoryPackCompressed = 1,

    /// <summary>
    /// System.Text.Json string.
    /// Human-readable, compatible with external tools. Slower.
    /// </summary>
    Json = 2
}