
using Android.OS;

namespace SkillStore.Service.Communication
{
	public class ServerCommBinder : Binder
	{
		public ServerCommService Service { get; private set; }

		public ServerCommBinder(ServerCommService service)
		{
			Service = service;
		}
	}
}