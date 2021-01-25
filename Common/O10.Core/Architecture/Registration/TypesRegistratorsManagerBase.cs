using System;
using System.Collections.Generic;

namespace O10.Core.Architecture.Registration
{
	internal abstract class TypesRegistratorsManagerBase
	{
		public IEnumerable<TypeRegistratorBase> GetAllRegistrators()
		{
			return DeduplicateRegistrators(GetTypeRegistrators());
		}

		protected IEnumerable<TypeRegistratorBase> DeduplicateRegistrators(IEnumerable<TypeRegistratorBase> typeRegistrators)
		{
			var deduplicated = new List<TypeRegistratorBase>();
			
			if (typeRegistrators == null)
			{
				return null;
			}

			var added = new HashSet<Type>();

			foreach (var registrator in typeRegistrators)
			{
				if (added.Contains(registrator.GetType()))
				{
					continue;
				}

				added.Add(registrator.GetType());

				deduplicated.Add(registrator);
			}

			return deduplicated;
		}

		protected abstract IEnumerable<TypeRegistratorBase> GetTypeRegistrators();
	}
}
