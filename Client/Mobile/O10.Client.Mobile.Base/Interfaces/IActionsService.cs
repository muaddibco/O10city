using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface IActionsService
    {
        string ResolveAction(string encoded);
    }
}
