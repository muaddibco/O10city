using System;
using System.Collections.Generic;

namespace O10.Core.Identity
{
    /// <summary>
    /// Generic key
    /// </summary>
    public interface IKey : IEqualityComparer<IKey>, IEquatable<IKey>
    {
        int Length { get; }

        Memory<byte> Value { get; set; }

        ArraySegment<byte> ArraySegment { get; }
    }
}
