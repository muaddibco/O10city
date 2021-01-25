using System.ComponentModel.Composition.Primitives;
using System.Reflection;


namespace O10.Core.Architecture.Registration
{
    public interface IRegistrationManager
    {
        RunMode CurrentRunMode { get; set; }
        void RegisterExtensionPoint(ExtensionPoint extensionPoint);
        void RegisterServiceContract(ServiceContract serviceContract);
        void RegisterType(RegisterType type);
        void AutoRegisterAssembly(Assembly assembly);
        void AutoRegisterUsingMefCatalog(ComposablePartCatalog catalog);
		void AutoRegisterUsingReflection();


        void CommitRegistrationsToContainer();
    }
}
