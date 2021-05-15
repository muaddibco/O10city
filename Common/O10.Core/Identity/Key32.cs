using System;
using O10.Core.ExtensionMethods;

namespace O10.Core.Identity
{
    /// <summary>
    /// Class represents Key with length of 32 bytes
    /// </summary>
    public class Key32 : KeyBase
    {
        public Key32()
            : base()
        {
        }

        public Key32(Memory<byte> value)
            : base(value)
        {
        }

        public override int Length => 32;

        public override bool Equals(IKey x, IKey y)
        {
            if (x == null && y == null)
            {
                return true;
            }


            if (!(x is Key32 pk1) || !(y is Key32 pk2))
            {
                return false;
            }

            return pk1.Value.Equals32(pk2.Value);
        }

        public override int GetHashCode(IKey obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return ((Key32)obj).Value.GetHashCode32();
        }

        public override bool Equals(object obj)
        {
            if(obj is byte[] arr && arr.Length == 32)
            {
                return Value.Equals32(arr);
            }

            return obj is Key32 pk && Value.Equals32(pk.Value);
        }

        public override bool Equals(IKey other)
        {
            if (other == null)
            {
                return false;
            }

            return Value.Equals32(other.Value);
        }
    }
}
