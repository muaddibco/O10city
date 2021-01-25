using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace O10.Core.Architecture.Registration
{
	internal class TypesRegistratorsManagerMef : TypesRegistratorsManagerBase
	{
		private readonly ComposablePartCatalog _catalog;

		[ImportMany]
		public IEnumerable<TypeRegistratorBase> _registrators { get; set; }

		public TypesRegistratorsManagerMef(ComposablePartCatalog catalog)
		{
			_catalog = catalog;
		}

		protected override IEnumerable<TypeRegistratorBase> GetTypeRegistrators()
		{
			//Create the CompositionContainer with the parts in the catalog
			var mefContainer = new CompositionContainer(_catalog);

			//Fill the imports of this object
			mefContainer.ComposeParts(this);

			return _registrators;
		}
	}
}
