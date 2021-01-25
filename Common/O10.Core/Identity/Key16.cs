using System;
using O10.Core.ExtensionMethods;

namespace O10.Core.Identity
{
    public class Key16 : IKey
    {
        private Memory<byte> _value;

        public Key16()
        {

        }

        public Key16(Memory<byte> value)
        {
            if (value.Length != Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Length must be of {Length} bytes");
            }

            Value = value;
        }

        /// <summary>
        /// Byte array of length of 16 bytes
        /// </summary>
        public Memory<byte> Value
        {
            get => _value;
            set
            {
                if(value.Length != Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Length must be of {Length} bytes");
                }

                _value = value;
                ArraySegment = _value.ToArraySegment();
            }
        }

        public int Length => 16;

        public ArraySegment<byte> ArraySegment { get; private set; }

        public bool Equals(IKey x, IKey y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (!(x is Key16 pk1) || !(y is Key16 pk2))
            {
                return false;
            }

            return pk1.Value.EqualsX16(pk2.Value);
        }

        public int GetHashCode(IKey obj)
        {
            return ((Key16)obj).Value.GetHashCode16();
        }

        public override int GetHashCode() => Value.GetHashCode16();

        public override string ToString() => Value.ToHexString();

        public override bool Equals(object obj)
        {
            if (!(obj is Key16 pk))
            {
                return false;
            }

            return Value.EqualsX16(pk.Value);
        }

        public bool Equals(IKey other)
        {
            if (other == null)
            {
                return false;
            }

            return Value.EqualsX16(other.Value);
        }
    }
}
