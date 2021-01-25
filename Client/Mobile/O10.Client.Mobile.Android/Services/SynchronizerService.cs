using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using O10.Client.Mobile.Base.Interfaces;
using System.Threading.Tasks.Dataflow;
using O10.Client.Mobile.Base.Models.StateNotifications;
using Android.Util;
using O10.Client.Common.Interfaces;
using O10.Client.Mobile.Base.Services;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xamarin.Forms;

namespace O10Wallet.Droid.Services
{
	[Service]
	public class SynchronizerService : Service
	{
		private static readonly string TAG = typeof(SynchronizerService).FullName;

		private readonly object _sync = new object();
		private IServiceProvider _serviceProvider;
		private BackgroundSynchronizer _backgroundSynchronizer;
		private IExecutionContext _executionContext;

		private IDisposable _stateNotificationUnsubscriber;
		private IDisposable _backgroundPacketsExtractorUnsubscriber;
		private bool _isStarted;
		private bool _initializeFrontendPipes = false;

		public IBinder Binder { get; private set; }

		#region Overrides

		public override IBinder OnBind(Intent intent)
		{
			Binder = new SynchronizerServiceBinder(this);
			return Binder;
		}

		public override void OnCreate()
		{
			base.OnCreate();
			Log.Info(TAG, "OnCreate: the service is initializing.");
		}

		[return: GeneratedEnum]
		public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
		{
			try
			{
				if (intent == null)
				{
					Log.Warn(TAG, "OnStartCommand: intent is null!");
					return StartCommandResult.Sticky;
				}

				Log.Info(TAG, $"OnStartCommand: intent.Action is {intent.Action}.");

				if (intent.Action?.Equals(Constants.ACTION_START_SERVICE) ?? false)
				{
					var notificationManager = NotificationManager.FromContext(ApplicationContext);

					notificationManager.Cancel(123);

					if (!_isStarted)
					{
						Log.Info(TAG, "OnStartCommand: The service is starting.");

						if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
						{
							RegisterForegroundServiceO();
						}
						else
						{
							RegisterForegroundService();
						}

						if (!Forms.IsInitialized)
						{
							Log.Info(TAG, "OnStartCommand: Forms.Init");
							Forms.Init(this, null);
						}

						InitializeServices(BackgroundBootstrapper.Initialize(CancellationToken.None));

						_isStarted = true;
					}
					else
					{
						Log.Info(TAG, "OnStartCommand: The service is already running.");
					}

					Log.Info(TAG, "OnStartCommand: _backgroundSynchronizer.Run()");
					_backgroundSynchronizer.Run();
					//string msg = intent.GetStringExtra("msg");
					//ShowSystemAlert(msg);
				}
				else if (intent.Action?.Equals(Constants.ACTION_STOP_SERVICE) ?? false)
				{
					Log.Info(TAG, "OnStartCommand: The service is stopping.");

					_backgroundSynchronizer?.Stop();
					_backgroundSynchronizer = null;
					_stateNotificationUnsubscriber?.Dispose();
					_stateNotificationUnsubscriber = null;
					_backgroundPacketsExtractorUnsubscriber?.Dispose();
					_backgroundPacketsExtractorUnsubscriber = null;

					StopForeground(true);
					StopSelf();

					_isStarted = false;
				}

			}
			catch (Exception ex)
			{
				Log.Error(TAG, ex.ToString());
				throw;
			}
			return StartCommandResult.Sticky;
		}

		private void ShowSystemAlert(string msg, int iconResourceId)
		{
			if (!string.IsNullOrEmpty(msg))
			{
				try
				{
					AlertDialog.Builder builder = new AlertDialog.Builder(this);
					builder.SetTitle("O10 Identity Notification");
					builder.SetIcon(iconResourceId);
					builder.SetMessage(msg);
					//NotificationAlertView notificationAlertView = new NotificationAlertView(this, null);
					//notificationAlertView.SetMessage(msg);
					//builder.SetView(notificationAlertView);
					var dialog = builder.Create();
					dialog.Window.SetType(Android.Views.WindowManagerTypes.SystemAlert);
					dialog.Show();
				}
				catch (Exception ex)
				{
					Log.Error(TAG, "Failed to show system alert", ex.Message);
				}
			}
		}

		private void InitializeServices(IServiceProvider serviceProvider)
		{
			Log.Info(TAG, "InitializeServices started");
			try
			{
				_serviceProvider = serviceProvider;

				_backgroundSynchronizer?.Stop();
				_backgroundSynchronizer = ActivatorUtilities.CreateInstance<BackgroundSynchronizer>(_serviceProvider);
				_backgroundSynchronizer.Initialize();

				SubscribeToNotifications();

				bool isProtectionEnabled = _serviceProvider.GetService<ICompromizationService>()?.IsProtectionEnabled ?? true;
				if (!isProtectionEnabled)
				{
					NotifyIsHackerAccount();
				}
				else
				{
					IAccountsService accountsService = _serviceProvider.GetService<IAccountsService>();
					var account = accountsService.GetAll().FirstOrDefault();

					if (account?.IsCompromised ?? false)
					{
						NotifyCompromized();
					}
					else
					{
						NotifyNoncompromized();
					}
				}

				Log.Info(TAG, "InitializeServices finished");
			}
			catch (Exception ex)
			{
				Log.Error(TAG, $"InitializeServices failed du to {ex.ToString()}");

				throw;
			}
		}

		public override void OnDestroy()
		{
			_backgroundSynchronizer?.Stop();
			_backgroundSynchronizer = null;
			_stateNotificationUnsubscriber?.Dispose();
			_stateNotificationUnsubscriber = null;
			_backgroundPacketsExtractorUnsubscriber?.Dispose();
			_backgroundPacketsExtractorUnsubscriber = null;

			var notificationManager = (NotificationManager)GetSystemService(NotificationService);
			notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);

			_isStarted = false;

			base.OnDestroy();
		}

		#endregion Overrides

		public void Initialize(IServiceProvider serviceProvider)
		{
			Log.Info(TAG, $"{nameof(SynchronizerService)}.{nameof(Initialize)} started");

			try
			{
				InitializeServices(serviceProvider);

				BindFrontendExecutionContext();
				Log.Info(TAG, $"{nameof(SynchronizerService)}.{nameof(Initialize)} finished");
			}
			catch (Exception ex)
			{
				Log.Error(TAG, $"{nameof(SynchronizerService)}.{nameof(Initialize)} failed due to {ex.ToString()}");
				Device.BeginInvokeOnMainThread(() => ShowSystemAlert($"{nameof(SynchronizerService)}.{nameof(Initialize)} failed due to {ex.Message}", Resource.Mipmap.icon));
			}
		}

		private void BindFrontendExecutionContext()
		{
			_executionContext = _serviceProvider.GetService<IExecutionContext>();
			_executionContext.InitializationCompleted.LinkTo(
				new ActionBlock<bool>(b =>
				{
					if (b)
					{
						InitializeFrontendPipes();
					}
				}));

			if(_executionContext.IsInitialized)
			{
				InitializeFrontendPipes();
			}
		}

		private void InitializeFrontendPipes()
		{
			if(_initializeFrontendPipes)
			{
				return;
			}

			lock (_sync)
			{
				if (_initializeFrontendPipes)
				{
					return;
				}

				_initializeFrontendPipes = true;

				Log.Info(TAG, $"{nameof(SynchronizerService)}.{nameof(InitializeFrontendPipes)} started");

				try
				{
					_backgroundPacketsExtractorUnsubscriber?.Dispose();
					_backgroundSynchronizer.InitializedEvent.Wait();
					_backgroundPacketsExtractorUnsubscriber = _executionContext.TransactionsService.GetSourcePipe<byte[]>().LinkTo(_backgroundSynchronizer.PacketsExtractor.GetTargetPipe<byte[]>());

					Log.Info(TAG, $"{nameof(SynchronizerService)}.{nameof(InitializeFrontendPipes)} completed");
				}
				catch (Exception ex)
				{
					Log.Error(TAG, $"{nameof(SynchronizerService)}.{nameof(InitializeFrontendPipes)} failed due to {ex.Message}");
				}
				finally
				{
					_initializeFrontendPipes = false;
				}
			}
		}

		private void SubscribeToNotifications()
		{
			IStateNotificationService stateNotificationService = _serviceProvider.GetService<IStateNotificationService>();
			_stateNotificationUnsubscriber?.Dispose();
			_stateNotificationUnsubscriber = stateNotificationService
				.NotificationsPipe
				.LinkTo(new ActionBlock<StateNotificationBase>(
					n =>
					{
						if (n is AccountCompomisedStateNotification)
						{
							if (!_backgroundSynchronizer.IsAccountCompromized())
							{
								NotifyCompromized();
								Device.BeginInvokeOnMainThread(() => ShowSystemAlert("Your Identity was compromized!", Resource.Drawable.ic_spyware));
							}
						}
						else if (n is AccountResetStateNotification)
						{
							NotifyNoncompromized();
							_backgroundSynchronizer.Stop();
							_backgroundSynchronizer.Initialize();
						}
						else if (n is RootAttributeDisabledStateNotification rootAttributeDisabled)
						{
							if(!_backgroundSynchronizer.IsAccountCompromized())
							{
								NotifyIdentityRevoked();
								Device.BeginInvokeOnMainThread(() => ShowSystemAlert("Your Identity was revoked!", Resource.Drawable.contact_red));
							}
						}
						else if (n is AccountModeChangedStateNotification accountModeChanged)
						{
							if (accountModeChanged.IsProtectionEnabled)
							{
								NotifyNoncompromized();
							}
							else
							{
								NotifyIsHackerAccount();
							}
						}
						else if (n is AccountOverridenStateNotification accountOverriden)
						{
							_backgroundSynchronizer.Stop();
							_backgroundSynchronizer.Initialize();
						}
					}));
		}

		/// <summary>
		/// Builds a PendingIntent that will display the main activity of the app. This is used when the 
		/// user taps on the notification; it will take them to the main activity of the app.
		/// </summary>
		/// <returns>The content intent.</returns>
		private PendingIntent BuildIntentToShowMainActivity()
		{
			var notificationIntent = new Intent(this, typeof(MainActivity));
			notificationIntent.SetAction(Constants.ACTION_MAIN_ACTIVITY);
			notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
			notificationIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);

			var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
			return pendingIntent;
		}

		private void RegisterForegroundService()
		{
			var notification = new NotificationCompat.Builder(this, Constants.NOTIFICATION_CHANNEL_ID)
				.SetContentTitle("O10 Network")
				.SetContentText("Packet Updater")
				.SetSmallIcon(Resource.Drawable.ic_security_shield)
				.SetContentIntent(BuildIntentToShowMainActivity())
				.SetChannelId(Constants.NOTIFICATION_CHANNEL_ID)
				.SetOngoing(true)
				//.AddAction(BuildRestartTimerAction())
				//.AddAction(BuildStopServiceAction())
				.Build();


			// Enlist this instance of the service as a foreground service
			StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
		}

		private void RegisterForegroundServiceO()
		{
			NotificationChannel chan = new NotificationChannel(Constants.NOTIFICATION_CHANNEL_ID, "O10 Network Channel", NotificationImportance.High)
			{
				LockscreenVisibility = NotificationVisibility.Public
			};

			NotificationManager manager = (NotificationManager)GetSystemService(NotificationService);

			manager.CreateNotificationChannel(chan);

			NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this, Constants.NOTIFICATION_CHANNEL_ID);

			Notification notification = notificationBuilder.SetOngoing(true)
				.SetContentTitle("O10 Network")
				.SetContentText("Account Guarded")
				.SetSmallIcon(Resource.Drawable.ic_security_shield)
				.SetChannelId(Constants.NOTIFICATION_CHANNEL_ID)
				.SetContentIntent(BuildIntentToShowMainActivity())
				.SetOngoing(true)
				.Build();

			StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
		}

		private void NotifyIsHackerAccount()
		{
			NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this, Constants.NOTIFICATION_CHANNEL_ID);

			Notification notification = notificationBuilder.SetOngoing(true)
				.SetContentTitle("Hacker Account")
				.SetSmallIcon(Resource.Drawable.ic_stat_hacker)
				.SetChannelId(Constants.NOTIFICATION_CHANNEL_ID)
				.SetContentIntent(BuildIntentToShowMainActivity())
				.Build();

			var notificationsManager = NotificationManagerCompat.From(this);
			notificationsManager.Notify(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
		}

		private void NotifyCompromized()
		{
			NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this, Constants.NOTIFICATION_CHANNEL_ID);

			Notification notification = notificationBuilder.SetOngoing(true)
				.SetContentTitle("O10 Network")
				.SetContentText("Account Compromized!")
				.SetSmallIcon(Resource.Drawable.ic_spyware)
				.SetChannelId(Constants.NOTIFICATION_CHANNEL_ID)
				.SetContentIntent(BuildIntentToShowMainActivity())
				.Build();

			var notificationsManager = NotificationManagerCompat.From(this);
			notificationsManager.Notify(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
		}

		private void NotifyNoncompromized()
		{
			NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this, Constants.NOTIFICATION_CHANNEL_ID);

			Notification notification = notificationBuilder.SetOngoing(true)
				.SetContentTitle("O10 Network")
				.SetContentText("Account Guarded")
				.SetSmallIcon(Resource.Drawable.ic_security_shield)
				.SetChannelId(Constants.NOTIFICATION_CHANNEL_ID)
				.SetContentIntent(BuildIntentToShowMainActivity())
				.Build();

			var notificationsManager = NotificationManagerCompat.From(this);
			notificationsManager.Notify(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
		}

		private void NotifyIdentityRevoked()
		{
			NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this, Constants.NOTIFICATION_CHANNEL_ID);

			Notification notification = notificationBuilder.SetOngoing(true)
				.SetContentTitle("Identity Revoked")
				.SetSmallIcon(Resource.Drawable.ic_stat_id_revoked)
				.SetChannelId(Constants.NOTIFICATION_CHANNEL_ID)
				.SetAutoCancel(true)
				.SetContentIntent(BuildIntentToShowMainActivity())
				.Build();

			var notificationsManager = NotificationManagerCompat.From(this);
			notificationsManager.Notify(Constants.IDENTITY_REVOKED_NOTIFICATION_ID, notification);
		}
	}
}