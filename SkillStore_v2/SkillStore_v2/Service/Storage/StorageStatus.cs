
namespace SkillStore.Service.Storage
{
	public enum StorageStatus
	{
		WriteFailed = 3001,
		ReadFailed,
		WriteSuccessful,
		ReadSuccessful,
		DataNotFound,
		DataFound,
		Starting,
		NoDataStored,
		DeleteSuccessful,
		DeleteFailed,
		StoreFailed,
		Stopped,
		StoreSuccessful
	}
}