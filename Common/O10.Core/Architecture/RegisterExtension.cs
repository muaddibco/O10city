using System;


namespace O10.Core.Architecture
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
    public class RegisterExtension : RegisterType
    {
        public RegisterExtension(Type implements, ExtensionOrderPriorities priority = ExtensionOrderPriorities.Normal)
        {
            Implements = implements;
            Lifetime = LifetimeManagement.Transient;
            Role = RegistrationRole.Extension;
            AllowsOverride = true;
            ExtensionOrderPriority = (int)priority;
        }

        public RegisterExtension(Type implements, double customPriority)
        {
            Implements = implements;
            Lifetime = LifetimeManagement.Transient;
            Role = RegistrationRole.Extension;
            AllowsOverride = true;
            ExtensionOrderPriority = customPriority;
        }
    }
}
