using System;


namespace O10.Core.Architecture
{
    /// <summary>
    /// Registers decorated class as default implementation of service specified by input argument "Implements" of the attribute constructor.
    /// </summary>
    /// <seealso cref="RegisterType" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
    public class RegisterDefaultImplementation : RegisterType
    {
        public RegisterDefaultImplementation(Type implements)
        {
            Implements = implements;
            Lifetime = LifetimeManagement.Transient;
            Role = RegistrationRole.DefaultImplementation;
            AllowsOverride = false;
        }
    }
}
