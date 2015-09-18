using System;
using Android.Media;
using Android.Util;
using Java.IO;
using Java.Lang;
using Exception = System.Exception;
using File = System.IO.File;
using Object = Java.Lang.Object;

namespace SkillStore.WavFileReader
{
	public class WavRecorder : Object, AudioRecord.IOnRecordPositionUpdateListener
	{
		private const string Tag = "WAVRecorder";
		private const int TimerInterval = 120;

		public Action<bool> Error;

		private AudioRecord _audioRecord;
		private RandomAccessFile _randomAccessWriter;
		private readonly object _randomAccessLock = new object();
		private string _filePath;
		private WavRecorderState _state;

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
				if (value == WavRecorderState.Error && Error != null)
				{
					Error(true);
				}
				_state = value;
			}
		}

		public bool IsRecording
		{
			get { return State == WavRecorderState.Recording; }
		}

		public bool IsInitializing
		{
			get { return State == WavRecorderState.Initializing; }
		}

		public bool IsReady
		{
			get { return State == WavRecorderState.Ready; }
		}

		public bool HasError
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
				if (bufferSize > 0)
				{
					return SampleRates.GetValueAt(i);
				}
			}

			return -1;
		}

		public static WavRecorder Instance(int sampleRate)
		{
			var wavRecorder = new WavRecorder(sampleRate);
			if (wavRecorder.State != WavRecorderState.Error)
				return new WavRecorder();
			return null;
		}

		public static WavRecorder Instance()
		{
			var wavRecorder = new WavRecorder();
			if (wavRecorder.State != WavRecorderState.Error)
				return new WavRecorder();
			return null;
		}

		private WavRecorder()
		{
			InitializeAudioRecord();
		}

		private WavRecorder(int sampleRate)
		{
			InitializeAudioRecord(sampleRate);
		}

		private void InitializeAudioRecord()
		{
			_audioRecord = FindAudioRecord();
			if (_audioRecord == null)
			{
				Log.Error(Tag, "No fitting audio record found.");
				State = WavRecorderState.Error;
				return;
			}

			_filePath = null;
			State = WavRecorderState.Initializing;
		}

		private void InitializeAudioRecord(int sampleRate)
		{
			var recorder = InitializeAudioRecorder(sampleRate);
			if (recorder != null && recorder.State == Android.Media.State.Initialized) return;

			Log.Error(Tag, "Failed to initialize AudioRecord with sampleRate: " + sampleRate);
			State = WavRecorderState.Error;
		}

		private AudioRecord FindAudioRecord()
		{
			for (var i = 0; i < SampleRates.NumberSampleRates; i++)
			{
				var recorder = InitializeAudioRecorder(SampleRates.GetValueAt(i));
				if (recorder != null && recorder.State == Android.Media.State.Initialized)
				{
					return recorder;
				}
			}

			Log.Error(Tag, "No AudioRecord for this device found.");
			State = WavRecorderState.Error;
			return null;
		}

		private AudioRecord InitializeAudioRecorder(int sampleRate)
		{
			try
			{
				_sampleRate = sampleRate;
				_framePeriod = _sampleRate * TimerInterval / 1000;
				_bufferSize = _framePeriod * 2 * FmtAvgBps * Channels / 8;

				if (_bufferSize < AudioRecord.GetMinBufferSize(sampleRate, ChannelIn.Mono, Encoding.Pcm16bit))
				{
					// Check to make sure buffer size is not smaller than the smallest allowed one 
					_bufferSize = AudioRecord.GetMinBufferSize(sampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
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
				Log.Error(Tag, e.Message);
				State = WavRecorderState.Error;
				return null;
			}
		}

		//private void InitializeAudioRecorder(int sampleRate)
		//{
		//	try
		//	{
		//		_sampleRate = sampleRate;
		//		_framePeriod = _sampleRate*TimerInterval/1000;
		//		_bufferSize = _framePeriod*2*FmtAvgBps*Channels/8;

		//		if (_bufferSize < AudioRecord.GetMinBufferSize(sampleRate, ChannelIn.Mono, Encoding.Pcm16bit))
		//		{
		//			// Check to make sure buffer size is not smaller than the smallest allowed one 
		//			_bufferSize = AudioRecord.GetMinBufferSize(sampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
		//			// Set frame period and timer interval accordingly
		//			_framePeriod = _bufferSize/(2*FmtAvgBps*Channels/8);
		//			Log.Debug(Tag, "Increasing buffer size to " + _bufferSize);
		//		}

		//		_audioRecord = new AudioRecord(AudioSource.Mic, _sampleRate, ChannelIn.Mono, Encoding.Pcm16bit, _bufferSize);

		//		//if (_audioRecord.State != Android.Media.State.Initialized)
		//		//	throw new Exception("AudioRecord initialization failed");
		//		if (_audioRecord.State == Android.Media.State.Initialized) 
		//		{ 
		//			_audioRecord.SetRecordPositionUpdateListener(this);
		//			_audioRecord.SetPositionNotificationPeriod(_framePeriod);
		//			//_filePath = null;
		//			//State = WavRecorderState.Initializing;
		//		}
		//		else
		//		{
		//			_audioRecord.Release();
		//			_audioRecord = null;
		//		}
				
		//	}
		//	catch (Exception e)
		//	{
		//		Log.Error(Tag, e.Message);
		//		State = WavRecorderState.Error;
		//	}
		//}

		public void Prepare()
		{
			if (!IsInitializing || _audioRecord.State != Android.Media.State.Initialized)
			{
				Log.Debug(Tag, "prepare() method called on uninitialized recorder");
				State = WavRecorderState.Error;
				return;
			}

			InitializeHeader();
			_buffer = new byte[_framePeriod*FmtAvgBps/8*Channels];
			State = WavRecorderState.Ready;

		}

		private void InitializeHeader()
		{

			try
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
				_randomAccessWriter.WriteInt(Integer.ReverseBytes(_sampleRate*FmtAvgBps*Channels/8));
				// Byte rate, SampleRate*NumberOfChannels*BitsPerSample/8
				_randomAccessWriter.WriteShort(Short.ReverseBytes(Channels*FmtAvgBps/8));
				// Block align, NumberOfChannels*BitsPerSample/8
				_randomAccessWriter.WriteShort(Short.ReverseBytes(FmtAvgBps)); // Bits per sample
				_randomAccessWriter.WriteBytes("data");
				_randomAccessWriter.WriteInt(0); // Data chunk size not known yet, write 0
			}
			catch (NullReferenceException e)
			{
				Log.Debug(Tag, e.Message);
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
				Log.Error(Tag, "Error occured in updateListener, recording is aborted" + e.Message);
				State = WavRecorderState.Error;
				//Stop();
			}
		}

		public void Start()
		{
			if (IsReady)
			{
				_payloadSize = 0;
				Log.Debug(Tag, "before StartRecording");
				try
				{
					_audioRecord.StartRecording();
					State = WavRecorderState.Recording;
				}
				catch (IllegalStateException e)
				{
					Log.Error(Tag, "Error while starting _audioRecord");
					Log.Error(Tag, e.Message + e.StackTrace);
					State = WavRecorderState.Error;
				}
				Log.Debug(Tag, "after StartRecording");
				//_audioRecord.Read(_buffer, 0, _buffer.Length);
				
			}
			else
			{
				Log.Error(Tag, "start() called on illegal state of WavRecorder.");
				State = WavRecorderState.Error;
			}
		}

		public void Stop()
		{
			if (!IsRecording) return;
			Log.Debug(Tag, "before StopRecording");
			if (_audioRecord.RecordingState == RecordState.Recording) _audioRecord.Stop();
			FinishHeader();
			Log.Debug(Tag, "after StopRecording");
			State = WavRecorderState.Stopped;
			
		}

		//public void Reset()
		//{
		//	try
		//	{
		//		if (HasError)
		//		{
		//			Log.Debug(Tag, "Recorder was in Error state while reset.");
		//			return;
		//		}
		//		Log.Debug(Tag, "before reset recorder");
		//		Release();
		//		_filePath = null;
		//		//_audioRecord = new AudioRecord(AudioSource.Mic, _sampleRate, ChannelIn.Mono, Encoding.Pcm16bit, _bufferSize);
		//		//State = WavRecorderState.Initializing;
		//		InitializeAudioRecord(_sampleRate);
		//		Log.Debug(Tag, "after reset recorder");
		//	}
		//	catch (Exception e)
		//	{
		//		Log.Error(Tag, e.Message);
		//		State = WavRecorderState.Error;
		//	}
		//}

		public void Release()
		{
			Log.Debug(Tag, "before release recorder");
			if (IsRecording)
			{
				Stop();
			}
			else if (IsReady)
			{
				try
				{
					lock (_randomAccessLock)
					{
						if (_randomAccessWriter != null)
						{
							_randomAccessWriter.Close();
							_randomAccessWriter = null;
						}
					}
				}
				catch (IOException e)
				{
					Log.Error(Tag, "I/O exception occured while closing output file" + e.Message);
				}
				File.Delete(_filePath);
			}

			if (_audioRecord != null)
			{
				_audioRecord.Release();
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
					_randomAccessWriter = null;
				}
			}
			catch (IOException e)
			{
				Log.Error(Tag, "I/O exception occured while closing output file " + e.Message);
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