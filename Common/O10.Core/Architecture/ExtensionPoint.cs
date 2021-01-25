using System;
using O10.Core.ExtensionMethods;

namespace O10.Core.Architecture
{
    /// <summary>
    /// Attribute decorating classes or interfaces and designating definition of extension point
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
    public class ExtensionPoint : Attribute
    {
        public Type Contract { get; set; }

        public ExtensionPoint()
        {
        }

        public ExtensionPoint(Type contract)
            : this()
        {
            Contract = contract;
        }

        public override string ToString()
        {
            return string.Format("Extension Point - {0}", Contract.FullNameWithAssemblyPath());
        }
    }
}
