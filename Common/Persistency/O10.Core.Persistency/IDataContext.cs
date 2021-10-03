using O10.Core.Architecture;

namespace O10.Core
{
	[ExtensionPoint]
	public interface IDataContext
	{
		string DataProvider { get; }

		IDataContext Initialize(string connectionString);

		IDataContext EnsureConfigurationCompleted();
	}
}
