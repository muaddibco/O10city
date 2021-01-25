using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Gateway.WebApp.Common.Services
{
    [ExtensionPoint]
    public interface INotificationService
    {
        Task Send(WitnessPackage witnessPackage);
    }
}
