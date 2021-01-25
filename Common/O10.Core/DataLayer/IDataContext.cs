using O10.Core.Architecture;

namespace O10.Core
{
	[ExtensionPoint]
	public interface IDataContext
	{
		string DataProvider { get; }

		void Initialize(string connectionString);

		void EnsureConfigurationCompleted();
	}
}
