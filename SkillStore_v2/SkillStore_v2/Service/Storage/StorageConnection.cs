using Android.Content;
using Android.OS;

namespace SkillStore.Service.Storage
{
	public class StorageConnection : ServiceConnection
	{
		public StorageConnection(ISkillStoreServiceConnectable connectable) : base(connectable) { }

		public override void OnServiceConnected(ComponentName name, IBinder service)
		{
			var binder = service as StorageBinder;
			if (binder == null) return;

			if (Connectable == null) return;

			Connectable.StorageBound = true;
			Connectable.OnServiceBound(binder);
		}

		public override void OnServiceDisconnected(ComponentName name)
		{
			if (Connectable == null) return;
			Connectable.StorageBound = false;
		}
	}
}