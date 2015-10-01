using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SkillStore.Main
{
	[Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/SkillStore.Theme.Application.Splash", 
			  ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask, NoHistory = true)]
	public class SkillStoreSplashActivity : Activity
	{

		private const int SplashCounter = 3;

		private int _splashCountdown;
		private Timer _splashTimer;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.SplashActivityLayout);
		}

		protected override void OnStart()
		{
			base.OnStart();

			ResetSplashTimer();
		}

		private void ResetSplashTimer()
		{
			if (_splashTimer != null)
			{
				_splashTimer.Dispose();
				_splashTimer = null;
			}
			_splashCountdown = SplashCounter;
			_splashTimer = new Timer(SplashTimerCallback, null, 0, 1000);
		}

		private void SplashTimerCallback(object state)
		{
			_splashCountdown--;

			if (_splashCountdown <= 0)
			{
				if (_splashTimer != null) _splashTimer.Dispose();
				StartActivity(typeof(TabbedActivity));
			}
		}
	}
}