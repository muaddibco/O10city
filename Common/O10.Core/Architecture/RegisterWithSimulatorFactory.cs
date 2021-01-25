using System;


namespace O10.Core.Architecture
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
    public class RegisterWithSimulatorFactory : RegisterType
    {
        public RegisterWithSimulatorFactory(Type factoryType)
        {
            Factory = factoryType;
            Lifetime = LifetimeManagement.Transient;
            Role = RegistrationRole.SimulatorImplementation;
            AllowsOverride = true;
        }
    }
}
