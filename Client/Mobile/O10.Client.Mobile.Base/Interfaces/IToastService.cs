using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface IToastService
    {
        void LongMessage(string message);
        void ShortMessage(string message);

    }
}
