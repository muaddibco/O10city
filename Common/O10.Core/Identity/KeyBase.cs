using O10.Core.ExtensionMethods;
using System;

namespace O10.Core.Identity
{
    public abstract class KeyBase : IKey
    {
        private Memory<byte> _value;

        public KeyBase()
        {

        }

        public KeyBase(Memory<byte> value)
        {
            if (value.Length != Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Length must be of {Length} bytes");
            }

            Value = value;
        }

        public abstract int Length { get; }

        public ArraySegment<byte> ArraySegment { get; private set; }

        /// <summary>
        /// Byte array of length of 16 bytes
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

        public abstract bool Equals(IKey x, IKey y);
        public abstract bool Equals(IKey other);
        public abstract bool Equals(byte[] other);
        public abstract bool Equals(Memory<byte> other);
        public abstract int GetHashCode(IKey obj);

        public override int GetHashCode() => GetHashCode(this);

        public override string ToString() => Value.ToHexString();
    }
}
