
using SkillStore.Service.Communication;
using SkillStore.Service.Recorder;
using SkillStore.Service.Storage;

namespace SkillStore.Service
{
	public interface ISkillStoreServiceConnectable
	{
		bool ServerCommBound { get; set; }
		bool StorageBound { get; set; }
		bool RecorderBound { get; set; }
		bool AllServicesBound { get; }

		ServerConnection ServerConnection { get; set; }
		ServerCommService ServerCommService { get; set; }
		StorageService StorageService { get; set; }
		StorageConnection StorageConnection { get; set; }
		AudioRecorderConnection AudioRecorderConnection { get; set; }
		AudioRecorderService AudioRecorderService { get; set; }


		void OnServiceBound(ServerCommBinder binder);
		void OnServiceBound(StorageBinder binder);
		void OnServiceBound(AudioRecorderBinder binder);
	}
}