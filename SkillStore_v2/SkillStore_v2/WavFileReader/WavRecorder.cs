using System;
using Android.Media;
using Android.Util;
using Java.IO;
using Java.Lang;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace SkillStore.WavFileReader
{
	public class WavRecorder : Object, AudioRecord.IOnRecordPositionUpdateListener
	{
		private const string Tag = "WAVRecorder";
		private const int TimerInterval = 120;
		
		private static WavRecorder _instance;

		public Action<bool> Error;

		
		private RandomAccessFile _randomAccessWriter;
		private readonly object _randomAccessLock = new object();
		private string _filePath;
		private WavRecorderState _state;
		private AudioRecord _audioRecord;
		private const short FmtAvgBps = 16;
		private const short Channels = 1;
		private int _sampleRate;
		private int _framePeriod;
		private int _bufferSize;
		private byte[] _buffer;
		private int _payloadSize;

		private WavRecorderState State
		{
			get { return _state; }
			set
			{
				_state = value;
				if (!HasError) return;

				if (Error != null) Error(true);
				Stop();
				Release();
			}
		}

		private bool IsRecording
		{
			get { return State == WavRecorderState.Recording; }
		}

		private bool IsInitializing
		{
			get { return State == WavRecorderState.Initializing; }
		}

		public bool IsReady
		{
			get { return State == WavRecorderState.Ready; }
		}

		private bool HasError
		{
			get { return State == WavRecorderState.Error; }
		}

		public bool IsStopped
		{
			get { return State == WavRecorderState.Stopped; }
		}

		public static int GetHighestSupportedSampleRate()
		{
			for (var i = 0; i < SampleRates.NumberSampleRates; i++)
			{
				var bufferSize = AudioRecord.GetMinBufferSize(SampleRates.GetValueAt(i), ChannelIn.Mono, Encoding.Pcm16bit);
				if (bufferSize > 0) return SampleRates.GetValueAt(i);
			}

			return -1;
		}

		public static WavRecorder Instance(int sampleRate = 0)
		{
			if (_instance != null)
			{
				if (_instance.IsRecording) _instance.Stop();
				_instance.Release();
				_instance = null;
			}

			_instance = sampleRate <= 0 ? new WavRecorder() : new WavRecorder(sampleRate);
			return _instance.State != WavRecorderState.Error ? _instance : null;
		}

		private WavRecorder()
		{
			var sampleRate = GetHighestSupportedSampleRate();
			InitializeAudioRecord(sampleRate);
		}

		private WavRecorder(int sampleRate)
		{
			InitializeAudioRecord(sampleRate);
		}

		private void InitializeAudioRecord(int sampleRate)
		{
			_audioRecord = CreateAudioRecord(sampleRate);
			if (_audioRecord != null && _audioRecord.State == Android.Media.State.Initialized)
			{
				State = WavRecorderState.Initializing;
				return;
			}

			Log.Error(Tag, "Failed to initialize AudioRecord with sampleRate: " + sampleRate);
			State = WavRecorderState.Error;
		}

		private AudioRecord CreateAudioRecord(int sampleRate)	//todo: refactor this
		{
			Thread.Sleep(200);

			try
			{
				_sampleRate = sampleRate;
				_framePeriod = _sampleRate * TimerInterval / 1000;
				_bufferSize = _framePeriod * 2 * FmtAvgBps * Channels / 8;

				var minBufferSize = AudioRecord.GetMinBufferSize(_sampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
				if (BufferSizeError(minBufferSize))
				{
					State = WavRecorderState.Error;
					return null;
				}

				if (_bufferSize < minBufferSize)
				{
					// Check to make sure buffer size is not smaller than the smallest allowed one 
					_bufferSize = AudioRecord.GetMinBufferSize(_sampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
					// Set frame period and timer interval accordingly
					_framePeriod = _bufferSize / (2 * FmtAvgBps * Channels / 8);
					Log.Debug(Tag, "Increasing buffer size to " + _bufferSize);
				}

				var audioRecord = new AudioRecord(AudioSource.Mic, _sampleRate, ChannelIn.Mono, Encoding.Pcm16bit, _bufferSize);
				audioRecord.SetRecordPositionUpdateListener(this);
				audioRecord.SetPositionNotificationPeriod(_framePeriod);
				return audioRecord;
			}
			catch (Exception e)
			{
				Log.Error(Tag, e.Message + e.StackTrace);
				State = WavRecorderState.Error;
				return null;
			}
		}

		private static bool BufferSizeError(int minBufferSize)
		{
			switch (minBufferSize)
			{
				case -1:
					Log.Error(Tag, "Implementation was unable to query the hardware for its input properties, or the minimum buffer size expressed in bytes.");
					return true;
				case -2:
					Log.Error(Tag, "Recording parameters not supported by the hardware, or an invalid parameter was passed.");
					return true;
			}
			return false;
		}

		public void Prepare()
		{
			if (!IsInitializing || _audioRecord.State != Android.Media.State.Initialized)
			{
				Log.Debug(Tag, "prepare() method called on uninitialized recorder");
				State = WavRecorderState.Error;
				return;
			}

			InitializeHeader();
			_buffer = new byte[_framePeriod * FmtAvgBps / 8 * Channels];
			State = WavRecorderState.Ready;

		}

		private void InitializeHeader()
		{
			try
			{
				lock (_randomAccessLock)
				{
					_randomAccessWriter = new RandomAccessFile(_filePath, "rw");

					_randomAccessWriter.SetLength(0);
					// Set file length to 0, to prevent unexpected behavior in case the file already existed
					_randomAccessWriter.WriteBytes("RIFF");
					_randomAccessWriter.WriteInt(0); // Final file size not known yet, write 0 
					_randomAccessWriter.WriteBytes("WAVE");
					_randomAccessWriter.WriteBytes("fmt ");
					_randomAccessWriter.WriteInt(Integer.ReverseBytes(16)); // Sub-chunk size, 16 for PCM
					_randomAccessWriter.WriteShort(Short.ReverseBytes(1)); // AudioFormat, 1 for PCM
					_randomAccessWriter.WriteShort(Short.ReverseBytes(Channels)); // Number of channels, 1 for mono, 2 for stereo
					_randomAccessWriter.WriteInt(Integer.ReverseBytes(_sampleRate)); // Sample rate
					_randomAccessWriter.WriteInt(Integer.ReverseBytes(_sampleRate * FmtAvgBps * Channels / 8));
					// Byte rate, SampleRate*NumberOfChannels*BitsPerSample/8
					_randomAccessWriter.WriteShort(Short.ReverseBytes(Channels * FmtAvgBps / 8));
					// Block align, NumberOfChannels*BitsPerSample/8
					_randomAccessWriter.WriteShort(Short.ReverseBytes(FmtAvgBps)); // Bits per sample
					_randomAccessWriter.WriteBytes("data");
					_randomAccessWriter.WriteInt(0); // Data chunk size not known yet, write 0
				}

			}
			catch (NullReferenceException e)
			{
				Log.Debug(Tag, e.Message + e.StackTrace);
				State = WavRecorderState.Error;
			}
		}

		public void OnMarkerReached(AudioRecord recorder)
		{
		}

		public void OnPeriodicNotification(AudioRecord recorder)
		{
			if (_audioRecord == null) return;

			_audioRecord.Read(_buffer, 0, _buffer.Length);
			try
			{
				lock (_randomAccessLock)
				{
					if (_randomAccessWriter == null) return;
					_randomAccessWriter.Write(_buffer);
					_payloadSize += _buffer.Length;
				}
			}
			catch (IOException e)
			{
				Log.Error(Tag, "Error occured in updateListener, recording is aborted" + e.Message + e.StackTrace);
				State = WavRecorderState.Error;
			}
		}

		public void Start()
		{
			if (!IsReady)
			{
				Log.Error(Tag, "start() called on illegal state of WavRecorder.");
				State = WavRecorderState.Error;
				return;
			}

			_payloadSize = 0;
			Log.Debug(Tag, "before StartRecording");
			try
			{
				Log.Debug(Tag, "AudioRecord State: " + _audioRecord.State);
				Log.Debug(Tag, "AudioRecord RecordingState: " + _audioRecord.RecordingState);
				_audioRecord.StartRecording();
				Log.Debug(Tag, "AudioRecord State: " + _audioRecord.State);
				Log.Debug(Tag, "AudioRecord RecordingState: " + _audioRecord.RecordingState);
				State = WavRecorderState.Recording;
			}
			catch (IllegalStateException e)
			{
				Log.Error(Tag, "Error while starting _audioRecord");
				Log.Error(Tag, e.Message + e.StackTrace);
				State = WavRecorderState.Error;
			}
			Log.Debug(Tag, "after StartRecording");
		}

		public void Stop()
		{
			if (IsRecording)
			{
				Log.Debug(Tag, "before StopRecording");
				if (_audioRecord != null && _audioRecord.RecordingState == RecordState.Recording) _audioRecord.Stop();
				FinishHeader();
				Log.Debug(Tag, "after StopRecording");
			}

			State = WavRecorderState.Stopped;
		}

		public void Release()
		{
			Log.Debug(Tag, "before release recorder");
			if (IsRecording)
			{
				Log.Error(Tag, "Release(): Tried to Release running WavRecorder. Please stop the recoder before release.");
				throw new IllegalStateException();
			}

			try
			{
				lock (_randomAccessLock)
				{
					if (_randomAccessWriter != null)
					{
						_randomAccessWriter.Close();
						_randomAccessWriter.Dispose();
						_randomAccessWriter = null;
					}
				}
			}
			catch (IOException e)
			{
				Log.Error(Tag, "Release(): I/O exception occured while closing output file" + e.Message + e.StackTrace);
			}

			//if(File.Exists(_filePath)) File.Delete(_filePath);

			if (_audioRecord != null)
			{
				_audioRecord.Stop();
				_audioRecord.Release();
				_audioRecord.Dispose();
				_audioRecord = null;
			}
			Log.Debug(Tag, "after release recorder");
		}

		private void FinishHeader()
		{
			try
			{
				lock (_randomAccessLock)
				{
					if (_randomAccessWriter == null) return;
					_randomAccessWriter.Seek(4); // Write size to RIFF header
					_randomAccessWriter.WriteInt(Integer.ReverseBytes(36 + _payloadSize));

					_randomAccessWriter.Seek(40); // Write size to Subchunk2Size field
					_randomAccessWriter.WriteInt(Integer.ReverseBytes(_payloadSize));

					_randomAccessWriter.Close();
					_randomAccessWriter.Dispose();
					_randomAccessWriter = null;
				}
			}
			catch (IOException e)
			{
				Log.Error(Tag, "I/O exception occured while closing output file " + e.Message + e.StackTrace);
				State = WavRecorderState.Error;
			}
		}

		public void SetOutputFile(string outputPath)
		{
			try
			{
				if (IsInitializing)
				{
					_filePath = outputPath;
				}
			}
			catch (Exception e)
			{
				Log.Error(Tag, e.Message);
				State = WavRecorderState.Error;
			}
		}
	}
}