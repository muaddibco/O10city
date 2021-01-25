using System;
using O10.Core.ExtensionMethods;

namespace O10.Core.Identity
{
    /// <summary>
    /// Class represents Key with length of 32 bytes
    /// </summary>
    public class Key32 : IKey
    {
        private Memory<byte> _value;

        public Key32()
        {

        }

        public Key32(Memory<byte> value)
        {
            if (value.Length != Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Length must be of {Length} bytes");
            }

            Value = value;
        }

        /// <summary>
        /// Byte array of length of 32 bytes
        /// </summary>
        public Memory<byte> Value
        {
            get => _value;
            set
            {
                if (value.Length != Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Length must be of {Length} bytes");
                }

                _value = value;
                ArraySegment = _value.ToArraySegment();
            }
        }

        public int Length => 32;

        public ArraySegment<byte> ArraySegment { get; private set; }

        public bool Equals(IKey x, IKey y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            Key32 pk1 = x as Key32;
            Key32 pk2 = y as Key32;

            if (pk1 == null || pk2 == null)
            {
                return false;
            }

            return pk1.Value.Equals32(pk2.Value);
        }

        public int GetHashCode(IKey obj)
        {
            return ((Key32)obj).Value.GetHashCode32();
        }

        public override int GetHashCode() => Value.GetHashCode32();

        public override string ToString() => Value.ToHexString();

        public override bool Equals(object obj)
        {

            if (!(obj is Key32 pk))
            {
                return false;
            }

            return Value.Equals32(pk.Value);
        }

        public bool Equals(IKey other)
        {
            if (other == null)
            {
                return false;
            }

            return Value.Equals32(other.Value);
        }
    }
}
