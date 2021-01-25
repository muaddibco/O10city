using System;
using O10.Core.ExtensionMethods;

namespace O10.Core.Architecture
{
    /// <summary>
    /// Attribute decorating classes or interfaces and designating definition for services
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
    public class ServiceContract : Attribute
    {
        public Type Contract { get; set; }

        public override string ToString()
        {
            return $"Service Contract - {Contract.FullNameWithAssemblyPath()}";
        }
    }
}
