using System;

namespace SkillStore.Service.Recorder
{
	public class AudioRecorderEventArgs : EventArgs
	{
		public AudioRecorderStatus Status { get; private set; }
		public string AudioFilePath { get; private set; }
		public int Counter { get; private set; }

		public bool HasOutputFile { get { return AudioFilePath != string.Empty; } }
		public bool IsCounting { get { return Counter > -1; } }

		public AudioRecorderEventArgs(AudioRecorderStatus status, string audioFilePath = "", int counter = -1)
		{
			Status = status;
			AudioFilePath = audioFilePath;
			Counter = counter;
		}
	}
}