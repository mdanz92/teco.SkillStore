using System;
using System.Collections.Generic;
using SkillStore.Utility;

namespace SkillStore.Service.Storage
{
	public class StorageEventArgs : EventArgs
	{
		public StorageStatus Status { get; private set; }
		public List<DataObject> Data { get; private set; }

		public bool HasData { get { return Data != null; } }


		public StorageEventArgs(StorageStatus status, List<DataObject> data = null)
		{
			Data = data;
			Status = status;
		}
	}
}