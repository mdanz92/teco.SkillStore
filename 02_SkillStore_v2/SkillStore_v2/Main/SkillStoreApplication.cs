using System;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Runtime;
using SkillStore.Service.Communication;
using SkillStore.Service.Recorder;
using SkillStore.Service.Storage;
using SkillStore.Utility;

namespace SkillStore.Main
{
#if DEBUG
	[Application(Debuggable = true, Label = "@string/ApplicationNameDebuggable")]
#else
    [Application (Debuggable = false, Label = "@string/ApplicationName")]
#endif
	public class SkillStoreApplication : Application
	{
		private static SkillStoreApplication _app;

		public static Context AppContext { get { return _app.ApplicationContext; } }
		public static DataObject StoredData { get; set; }
		public static string Version { get; private set; }
		public static bool IsHelpRequired
		{
			get
			{
				var preferences = PreferenceManager.GetDefaultSharedPreferences(AppContext);
				return !(preferences.Contains(SettingsKey.ShowAgainPreference) &&
					   preferences.GetBoolean(SettingsKey.ShowAgainPreference, false));
			}
		}

		public SkillStoreApplication (IntPtr ptr, JniHandleOwnership ownership) : base (ptr, ownership) { }

		public override void OnCreate()
		{
			base.OnCreate();
			_app = this;

			var pInfo = PackageManager.GetPackageInfo(PackageName, 0);
			Version = pInfo.VersionName;

			StartServices();
		}

		public override void OnTerminate()
		{
			StopServices();

			base.OnTerminate();
		}

		private void StartServices()
		{
			StartService(new Intent(this, typeof(ServerCommService)));
			StartService(new Intent(this, typeof(StorageService)));
			StartService(new Intent(this, typeof(AudioRecorderService)));
		}

		private void StopServices()
		{
			StopService(new Intent("Skillstore.Service.Communication.ServerCommService"));
			StopService(new Intent("Skillstore.Service.AudioRecorder.AudioRecorderService"));
			StopService(new Intent("Skillstore.Service.Storage.StorageService"));
		}
	}
}