
using System.Threading;

namespace SkillStore.Utility
{
	public class OneShotTimer
	{
		private readonly Timer _timer;
		private readonly TimerCallback _callback;

		public static OneShotTimer Shot(TimerCallback callback, int dueTime)
		{
			return new OneShotTimer(callback, dueTime);
		}

		private OneShotTimer(TimerCallback callback, int dueTime)
		{
			_callback = callback;
			_timer = new Timer(InternalCallback, null, dueTime, -1);
		}

		private void InternalCallback(object state)
		{
			_callback.Invoke(state);
			_timer.Dispose();
		}

	}
}