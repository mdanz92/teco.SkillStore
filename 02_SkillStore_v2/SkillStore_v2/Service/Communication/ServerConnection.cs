using Android.Content;
using Android.OS;

namespace SkillStore.Service.Communication
{
	public class ServerConnection : ServiceConnection
	{
		public ServerConnection(ISkillStoreServiceConnectable connectable) : base(connectable) { }

		public override void OnServiceConnected(ComponentName name, IBinder service)
		{
			var binder = service as ServerCommBinder;
			if (binder == null) return;
			
			if (Connectable == null) return;
			
			Connectable.ServerCommBound = true;
			Connectable.OnServiceBound(binder);
		}

		public override void OnServiceDisconnected(ComponentName name)
		{
			if (Connectable == null) return;
			Connectable.ServerCommBound = false;
		}
	}
}