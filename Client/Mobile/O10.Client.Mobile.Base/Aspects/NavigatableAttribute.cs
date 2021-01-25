using System;

namespace O10.Client.Mobile.Base.Aspects
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NavigatableAttribute : Attribute
    {
        public NavigatableAttribute(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; }
    }
}
