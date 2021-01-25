using System.Net.Http;
using System.Threading.Tasks;
using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Services.Inherence
{
    [ServiceContract]
    public interface IO10InherenceService
    {
        Task<HttpResponseMessage> RequestO10InherenceServer(byte[] sessionKey, (byte[] commitment, byte[] image)[] commitmentImages);
    }
}
