using O10.Core;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
	public interface IWitnessPackagesProviderRepository : IRepository<IWitnessPackagesProvider, string>
	{
	}
}
