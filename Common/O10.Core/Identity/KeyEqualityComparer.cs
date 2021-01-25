using System;
using System.Collections.Generic;

namespace O10.Core.Identity
{
	public class KeyEqualityComparer : IEqualityComparer<IKey>
	{
		public bool Equals(IKey x, IKey y)
		{
			if(x != null && y != null)
			{
				if (x.Length == y.Length)
				{
					return x.Equals(y);
				}
			}

			return false;
		}

		public int GetHashCode(IKey key)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			return key.GetHashCode();
		}
	}
}
