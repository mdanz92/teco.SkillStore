using System;
using Android.Content;
using Android.OS;
using Object = Java.Lang.Object;

namespace SkillStore.Service
{
	public class ServiceConnection : Object, IServiceConnection
	{
		protected ISkillStoreServiceConnectable Connectable { get; private set; }

		protected ServiceConnection(ISkillStoreServiceConnectable connectable)
		{
			Connectable = connectable;
		}

		public virtual void OnServiceConnected(ComponentName name, IBinder service)
		{
			throw new NotImplementedException();
		}

		public virtual void OnServiceDisconnected(ComponentName name)
		{
			throw new NotImplementedException();
		}
	}
}