using System;

namespace O10.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OptionalAttribute : Attribute
    {
    }
}
