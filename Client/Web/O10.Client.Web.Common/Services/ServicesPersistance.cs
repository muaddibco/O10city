using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace O10.Client.Web.Common.Services
{
    public class Persistency
	{
        public Persistency(long accountId, IServiceProvider serviceProvider)
        {
            AccountId = accountId;
            Scope = serviceProvider.CreateScope();
            CancellationTokenSource = new CancellationTokenSource();
        }

		public long AccountId { get; }
        public IServiceScope Scope { get; }
        
		public CancellationTokenSource CancellationTokenSource { get; }
	}
}
