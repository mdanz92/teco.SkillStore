
namespace SkillStore.Service.Recorder
{
	public enum AudioRecorderStatus
	{
		Starting = 2001,
		Preparing,
		Recording,
		FinishedPreparing,
		StoppedRecording,
		RecordingFailed,
		RecordingSuccessful,
		UpdatePrepareCounter,
		UpdateRecordingCounter,
		Stopped,
		CheckFailed,
		CheckingSamples,
		CheckSuccessful,
		RecorderInitializationError
	}
}