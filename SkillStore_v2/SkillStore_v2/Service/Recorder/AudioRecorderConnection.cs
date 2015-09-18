using Android.Content;
using Android.OS;

namespace SkillStore.Service.Recorder
{
	public class AudioRecorderConnection : ServiceConnection
	{
		public AudioRecorderConnection(ISkillStoreServiceConnectable connectable) : base(connectable) { }

		public override void OnServiceConnected(ComponentName name, IBinder service)
		{
			var binder = service as AudioRecorderBinder;
			if (binder == null) return;

			if (Connectable == null) return;

			Connectable.RecorderBound = true;
			Connectable.OnServiceBound(binder);
		}

		public override void OnServiceDisconnected(ComponentName name)
		{
			if (Connectable == null) return;
			Connectable.RecorderBound = false;
		}
	}
}