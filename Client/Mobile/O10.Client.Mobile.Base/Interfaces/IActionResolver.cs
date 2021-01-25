using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ExtensionPoint]
    public interface IActionResolver
    {
        string ResolveAction(string action);
    }
}
