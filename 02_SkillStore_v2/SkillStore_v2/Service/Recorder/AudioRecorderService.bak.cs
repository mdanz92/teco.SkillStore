
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using SkillStore.WavFileReader;
using File = Java.IO.File;
using Thread = Java.Lang.Thread;

namespace SkillStore.Service.Recorder
{

	public delegate void AudioRecorderEventHandler(object s, AudioRecorderEventArgs e);

	[Service]
	[IntentFilter(new[] { "Skillstore.Service.AudioRecorder.AudioRecorderService" })]
	public class AudioRecorderService : Android.App.Service
	{
		private const string Tag = "AudioRecorderService";

		private string _audioFilePath;
		private int _counter;
		private AudioRecorderStatus _status = AudioRecorderStatus.Stopped;

		public event AudioRecorderEventHandler Communication;

		private RecorderThread WorkerThread { get; set; }

		private void OnCommunication(object s, AudioRecorderEventArgs e)
		{
			var handler = Communication;
			if (handler != null) handler(s, e);
		}
		public AudioRecorderStatus Status
		{
			get { return _status; }
			private set
			{
				_status = value;
				OnCommunication(this, new AudioRecorderEventArgs(_status, _audioFilePath, _counter));
			}
		}

		public override IBinder OnBind(Intent intent)
		{
			return new AudioRecorderBinder(this);
		}

		public void StartRecorder(string id)
		{
			StopRecorder();
			Status = AudioRecorderStatus.Starting;

			WorkerThread = new RecorderThread(this, id);
			WorkerThread.Name = "RecorderThread";
			WorkerThread.Start();

			Log.Debug(Tag, "Recorder started.");
		}

		public void StopRecorder()
		{
			if (WorkerThread != null)
			{
				WorkerThread.RequestStop();
				WorkerThread.Join();
				if (!WorkerThread.IsAlive)
					WorkerThread = null;
			}

			Log.Debug(Tag, "Recorder stopped");
		}

		#region Service work thread

		private class RecorderThread : Thread
		{
			private const int RecordingTime = 5;
			private const string ThreadTag = "AudioRecorderThread";
			private const string TmpPath = "tmp/"; 

			private readonly AudioRecorderService _service;
			private readonly string _fileName;
			private List<string> _samplePaths;
			private WavRecorder _recorder;
			private Timer _prepareTimer;
			private Timer _recordingTimer;
			private int _prepareCountdown = 4;
			private int _recordingCountdown;
			private string _storagePath;
			private int _sampleRate;


			private bool IsPreparing { get; set; }
			private bool IsRecording { get; set; }
			private bool IsStopRequested { get; set; }

			public RecorderThread(AudioRecorderService service, string id)
			{
				_service = service;
				_fileName = id;

				InitializeSamplePaths();
			}

			private void SetupStorageDir()
			{
				if (Environment.ExternalStorageDirectory.Path != null)
					_storagePath = Environment.ExternalStorageDirectory.Path + File.Separator + _service.Resources.GetString(Resource.String.PathOnDevice);
				else
					_storagePath = _service.FilesDir.Path + _service.Resources.GetString(Resource.String.PathOnDevice);

				if (!Directory.Exists(_storagePath + TmpPath)) Directory.CreateDirectory(_storagePath + TmpPath);
			}

			private void InitializeSamplePaths()
			{
				SetupStorageDir();

				_samplePaths = new List<string>();
				for (int i = 0; i < 3; i++)
				{
					_samplePaths.Add(_storagePath + TmpPath + _fileName + "_" + i + ".wav");
				}
			}

			public override void Run()
			{
				base.Run();

				_sampleRate = WavRecorder.GetHighestSupportedSampleRate();
				if (_sampleRate <= 0)
				{
					Log.Error(ThreadTag, "Failed to find a fitting sample rate. Can't initialize WavRecorder");
					_service.RecorderInitializationError();
					RequestStop();
				}

				foreach (var path in _samplePaths)
				{
					if (IsStopRequested) break;
					RecordAudio(path);
					while (!IsStopRequested && (IsPreparing || IsRecording))
					{
						Sleep(200);
						if (IsPreparing) Log.Debug(ThreadTag, "Preparing...");
						if (IsRecording) Log.Debug(ThreadTag, "Recording...");
					}
				}

				Log.Debug(ThreadTag, "Finished recording");
				if (!IsStopRequested)
					ExportAudioFile();

				while (!IsStopRequested) Sleep(200);
				_service.Stopped();
			}

			private void RecordAudio(string path)
			{
				//_recorder = WavRecorder.Instance();
				_recorder = WavRecorder.Instance(_sampleRate);
				if (_recorder == null)
				{
					Log.Error(ThreadTag, "WavRecorder instance was null.");
					_service.RecordingFailed();
					RequestStop();
					return;
				}
				_recorder.Error = OnWavRecorderError;
				_recorder.SetOutputFile(path);
				_recorder.Prepare();
				
				ResetPrepareTimer();
			}

			private void OnWavRecorderError(bool errorOccured)
			{
				if (!errorOccured) return;
				Log.Error(ThreadTag, "Recording with WAVRecorder failed.");
				_service.RecordingFailed();
				RequestStop();
			}

			private void ResetPrepareTimer()
			{
				if (_prepareTimer != null)
				{
					_prepareTimer.Dispose();
					_prepareTimer = null;
					_prepareCountdown = 4;
				}
				_prepareTimer = new Timer(PrepareTimerCallback, null, 0, 1000);
				IsPreparing = true;
				_service.Preparing();
			}

			private void PrepareTimerCallback(object state)
			{
				_prepareCountdown--;
				_service.UpdatePrepareCounter(_prepareCountdown);

				if (IsStopRequested || _prepareCountdown <= 0)
				{
					if (_prepareTimer != null) _prepareTimer.Dispose();
					if (_recorder != null) _recorder.Start();
					_service.FinishedPreparing();
					IsPreparing = false;
					if (!IsStopRequested) ResetRecordingTimer();
				}
			}

			private void ResetRecordingTimer()
			{
				if (_recordingTimer != null)
				{
					_recordingTimer.Dispose();
					_recordingTimer = null;
					_recordingCountdown = 0;
				}

				_recordingTimer = new Timer(RecordingTimerCallback, null, 0, 1000);
				IsRecording = true;
				_service.Recording();
			}

			private void RecordingTimerCallback(object state)
			{
				_recordingCountdown++;
				_service.UpdateRecordingCounter(_recordingCountdown);

				if (IsStopRequested || _recordingCountdown >= RecordingTime)
				{
					if (_recordingTimer != null) _recordingTimer.Dispose();
					if (_recorder != null)
					{
						_recorder.Stop();
						_recorder.Release();
						_recorder = null;
					}
					IsRecording = false;
					_service.StoppedRecording();
				}
			}

			private bool CheckSamples()
			{
				_service.CheckingSamples();
				
				var dataList = new List<byte[]>();
				_samplePaths.ForEach(samplePath => dataList.Add(new WavFile(samplePath).Data));
				return SampleEvaluator.SampleEvaluator.CheckSamples(dataList);
			}

			private void ExportAudioFile() //TODO
			{
				if (!CheckSamples())
				{
					Log.Error(ThreadTag, "CheckSamples failed.");
					_service.CheckFailed();
					RequestStop();
					return;
				}
				_service.CheckSuccessful();
				
				var returnFilePath = MergeSamples();//EncodeToFlac(MergeSamples());
				if (string.IsNullOrEmpty(returnFilePath))
				{
					Log.Error(ThreadTag, "Merge or conversion to .flac failed.");
					_service.RecorderInitializationError();
					//_service.RecordingFailed();
					RequestStop();
					return;
				}

				var durationOk = CheckMergedSampleDuration(returnFilePath);
				if (!durationOk)
				{
					Log.Error(ThreadTag, "Merged file was too short.");
					_service.RecorderInitializationError();
					RequestStop();
					return;
				}

				_service.RecordingSuccessful(returnFilePath);
				RequestStop();
			}

			private string MergeSamples()
			{
				var mergedFilePath = _storagePath + TmpPath + _fileName + ".wav";
				var mergedFile = new WavFile(mergedFilePath);

				if (_samplePaths != null)
				{
					//_samplePaths.ForEach(samplePath => mergedFile.AppendFile(new WavFile(samplePath)));

					if (_samplePaths.Select(samplePath => mergedFile.AppendFile(new WavFile(samplePath))).Any(flag => !flag))
					{
						Log.Error(ThreadTag, "Merge failed.");
						return null;
					}
					if (System.IO.File.Exists(mergedFilePath)) return mergedFilePath;

					Log.Error(ThreadTag, "Merged file does not exist after file merge.");
					return null;
				}

				Log.Error(ThreadTag, "_samplePaths was null in MergeSamples");
				return null;
			}

			private static bool CheckMergedSampleDuration(string filePath )
			{
				var file = new WavFile(filePath);
				return file.Duration > 10;
			}

			//private string EncodeToFlac(string inputFilePath)
			//{
			//	if (string.IsNullOrEmpty(inputFilePath))
			//	{
			//		Log.Error(Tag, "inputFilePath was null or empty in EncodeToFlac.");
			//		return null;
			//	}
				
			//	var flacEncoder = new FLAC_FileEncoder();
			//	File outputFile;
			//	using (var inputFile = new File(inputFilePath))
			//	{
			//		var outputFileName = inputFile.Name.Replace(".wav", ".flac");
			//		outputFile = new File(inputFile.Parent, outputFileName);
			//		flacEncoder.Encode(inputFile, outputFile);
			//	}

			//	return outputFile.AbsolutePath;
			//}

			public void RequestStop()
			{
				IsStopRequested = true;
				if (_recorder != null)
				{
					_recorder.Stop();
					_recorder.Release();
					_recorder = null;
				}

				DeleteUnfinishedFiles(_storagePath, _fileName);
			}

			private static void DeleteUnfinishedFiles(string storagePath, string fileName)
			{
				for (var i = 0; i < 3; i++)
				{
					System.IO.File.Delete(storagePath + TmpPath + fileName + "_" + i + ".wav");
				}
			}
		}

		#endregion

		#region Status callbacks

		private void Preparing()
		{
			Status = AudioRecorderStatus.Preparing;
		}

		private void UpdatePrepareCounter(int prepareCountdown)
		{
			_counter = prepareCountdown;
			Status = AudioRecorderStatus.UpdatePrepareCounter;
		}

		private void FinishedPreparing()
		{
			Status = AudioRecorderStatus.FinishedPreparing;
		}

		private void Recording()
		{
			Status = AudioRecorderStatus.Recording;
		}

		private void UpdateRecordingCounter(int recordingCountdown)
		{
			_counter = recordingCountdown;
			Status = AudioRecorderStatus.UpdateRecordingCounter;
		}

		private void StoppedRecording()
		{
			Status = AudioRecorderStatus.StoppedRecording;
		}

		private void RecordingSuccessful(string returnFilePath)
		{
			_audioFilePath = returnFilePath;
			Status = AudioRecorderStatus.RecordingSuccessful;
		}

		private void RecordingFailed()
		{
			Status = AudioRecorderStatus.RecordingFailed;
		}

		private void CheckFailed()
		{
			Status = AudioRecorderStatus.CheckFailed;
		}

		private void CheckingSamples()
		{
			Status = AudioRecorderStatus.CheckingSamples;
		}

		private void CheckSuccessful()
		{
			Status = AudioRecorderStatus.CheckSuccessful;
		}

		private void Stopped()
		{
			Status = AudioRecorderStatus.Stopped;
		}

		private void RecorderInitializationError()
		{
			Status = AudioRecorderStatus.RecorderInitializationError;
		}

		#endregion

	}
}