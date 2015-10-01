
using Android.OS;

namespace SkillStore.Service.Recorder
{
	public class AudioRecorderBinder : Binder
	{

		public AudioRecorderService Service { get; private set; }

		public AudioRecorderBinder(AudioRecorderService service)
		{
			Service = service;
		}
	}
}