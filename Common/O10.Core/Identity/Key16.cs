using System;
using O10.Core.ExtensionMethods;

namespace O10.Core.Identity
{
    public class Key16 : KeyBase
    {

        public Key16()
            : base()
        {
        }

        public Key16(Memory<byte> value)
            : base(value)
        {
        }

        public override int Length => 16;

        public override bool Equals(IKey x, IKey y)
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

        public override int GetHashCode(IKey obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return ((Key16)obj).Value.GetHashCode16();
        }


        public override bool Equals(object obj)
        {
            if (obj is byte[] arr && arr.Length == 16)
            {
                return Value.EqualsX16(arr);
            }

            return obj is Key16 pk && Value.EqualsX16(pk.Value);
        }

        public override bool Equals(IKey other) => other != null && other.Length == Length && Value.EqualsX16(other.Value);

        public override bool Equals(byte[] other) => other != null && other.Length == Length && Value.EqualsX16(other);

        public override bool Equals(Memory<byte> other) => other.Length == 16 && Value.EqualsX16(other);
    }
}
