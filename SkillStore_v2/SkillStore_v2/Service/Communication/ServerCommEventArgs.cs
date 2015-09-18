using System;
using SkillStore.Utility;

namespace SkillStore.Service.Communication
{
	public class ServerCommEventArgs : EventArgs
	{
		public ServerStatus Status { get; private set; }
		public DataObject Data { get; private set; }

		public bool HasData { get { return Data != null; } }

		public ServerCommEventArgs(ServerStatus status, DataObject data = null)
		{
			Status = status;
			Data = data;
		}
	}
}