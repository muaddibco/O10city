using Android.OS;
using System;
using O10.Client.Mobile.Base.Interfaces;

namespace O10Wallet.Droid.Services
{
	public class SynchronizerServiceBinder : Binder, ISynchronizerServiceBinder
	{
		private readonly SynchronizerService _synchronizerService;

		public SynchronizerServiceBinder(SynchronizerService synchronizerService)
		{
			_synchronizerService = synchronizerService;
		}

		public void Initialize(IServiceProvider serviceProvider) 
			=> _synchronizerService?.Initialize(serviceProvider);
	}
}