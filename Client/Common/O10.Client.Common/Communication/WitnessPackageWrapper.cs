using System.Threading.Tasks;
using O10.Core.Models;

namespace O10.Client.Common.Communication
{
    public class WitnessPackageWrapper
    {
        public WitnessPackageWrapper(WitnessPackage witnessPackage)
        {
            WitnessPackage = witnessPackage;
            CompletionSource = new TaskCompletionSource<bool>();
        }

        public TaskCompletionSource<bool> CompletionSource { get; }

        public WitnessPackage WitnessPackage { get; }
    }
}
