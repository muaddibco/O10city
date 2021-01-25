using System.ComponentModel.Composition;
using O10.Core.Architecture.Registration;

namespace O10.Core.Architecture
{
    [InheritedExport]
    public abstract class TypeRegistratorBase
    {
        public virtual void Register(IRegistrationManager registrationManager)
        {
            registrationManager.AutoRegisterAssembly(GetType().Assembly);
        }
    }
}
