using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace O10.Core.Architecture.Registration
{
	internal class TypesRegistratorsManagerReflection : TypesRegistratorsManagerBase
	{
		protected override IEnumerable<TypeRegistratorBase> GetTypeRegistrators()
		{
            List<Type> registratorsTypes = FindTypes(t => !(t?.IsAbstract ?? true) && typeof(TypeRegistratorBase).IsAssignableFrom(t)).ToList();

			List<TypeRegistratorBase> typeRegistrators = new List<TypeRegistratorBase>();
			foreach (var type in registratorsTypes)
			{
				TypeRegistratorBase typeRegistratorBase = (TypeRegistratorBase)Activator.CreateInstance(type);
				typeRegistrators.Add(typeRegistratorBase);
			}

			return typeRegistrators;
		}

		public static IEnumerable<Type> FindTypes(Func<Type, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (!assembly.IsDynamic)
				{
					Type[] exportedTypes = null;
					try
					{
						exportedTypes = assembly.GetExportedTypes();
					}
					catch (ReflectionTypeLoadException e)
					{
						exportedTypes = e.Types;
					}

					if (exportedTypes != null)
					{
						foreach (var type in exportedTypes)
						{
							if (predicate(type))
								yield return type;
						}
					}
				}
			}
		}
	}
}
