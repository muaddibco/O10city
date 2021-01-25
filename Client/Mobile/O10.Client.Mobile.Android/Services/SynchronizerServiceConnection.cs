using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using O10.Client.Mobile.Base.Interfaces;

namespace O10Wallet.Droid.Services
{
	public class SynchronizerServiceConnection : Java.Lang.Object, IServiceConnection
	{
		static readonly string TAG = typeof(SynchronizerServiceConnection).FullName;
		private readonly MainActivity _mainActivity;

		public SynchronizerServiceConnection(MainActivity mainActivity)
		{
			IsConnected = false;
			Binder = null;
			_mainActivity = mainActivity;
		}

		public bool IsConnected { get; set; }
		public SynchronizerServiceBinder Binder { get; set; }

		public void OnServiceConnected(ComponentName name, IBinder service)
		{
			Binder = service as SynchronizerServiceBinder;
			IsConnected = Binder != null;

			string message = "onServiceConnected - ";
			Log.Debug(TAG, $"OnServiceConnected {name.ClassName}");

			if (IsConnected)
			{
				message = message + " bound to service " + name.ClassName;
				_mainActivity.OnSynchronizerConnected(Binder);
			}
			else
			{
				message = message + " not bound to service " + name.ClassName;
				//mainActivity.UpdateUiForUnboundService();
			}

			Log.Info(TAG, message);
		}

		public void OnServiceDisconnected(ComponentName name)
		{
			Log.Debug(TAG, $"OnServiceDisconnected {name.ClassName}");
			IsConnected = false;
			Binder = null;
			//mainActivity.UpdateUiForUnboundService();
		}
	}
}