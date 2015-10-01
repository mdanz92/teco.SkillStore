
using Android.OS;

namespace SkillStore.Service.Storage
{
	public class StorageBinder : Binder
	{
		public StorageService Service { get; private set; }

		public StorageBinder(StorageService service)
		{
			Service = service;
		}
	}
}