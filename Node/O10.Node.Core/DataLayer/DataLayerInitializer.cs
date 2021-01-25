using System;
using System.Threading;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Node.Core.DataLayer
{
    public class DataLayerInitializer : InitializerBase
    {
        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Highest7;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
