using System;

namespace O10Wallet.Droid.Services
{
	public class UtcTimestamper
	{
		DateTime _startTime;
		bool _wasReset = false;

		public UtcTimestamper()
		{
			_startTime = DateTime.UtcNow;
		}

		public string GetFormattedTimestamp()
		{
			TimeSpan duration = DateTime.UtcNow.Subtract(_startTime);

			return _wasReset ? $"Service restarted at {_startTime} ({duration:c} ago)." : $"Service started at {_startTime} ({duration:c} ago).";
		}

		public void Restart()
		{
			_startTime = DateTime.UtcNow;
			_wasReset = true;
		}
	}
}