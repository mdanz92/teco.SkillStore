using System;
using System.IO;
using System.Linq;
using Android.Util;

namespace SkillStore.WavFileReader
{
	public class WavFile
	{
		private const string Tag = "WavFile";

		public string AbsolutPath { get; private set; }
		public byte[] ChunkId { get; private set; }
		public uint FileSize { get; private set; }
		public byte[] RiffType { get; private set; }
		public byte[] FmtId { get; private set; }
		public uint FmtSize { get; private set; }
		public ushort FmtCode { get; private set; }
		public ushort Channels { get; private set; }
		public uint SampleRate { get; private set; }
		public uint FmtAvgBps { get; private set; }
		public ushort FmtBlockAlign { get; private set; }
		public ushort BitDepth { get; private set; }
		public int FmtExtraSize { get; private set; }
		public byte[] DataId { get; private set; }
		public uint DataSize { get; private set; }
		public byte[] Data { get; private set; }
		public int Duration { get { return (int) (Data.Length / SampleRate / 2); } }


		public WavFile(string filePath)
		{
			AbsolutPath = filePath;
			using (var wavfileStream = File.Open(AbsolutPath, FileMode.OpenOrCreate, FileAccess.Read))
			{
				if (wavfileStream.Length == 0)
				{
					wavfileStream.Close();
					return;
				}

				using (var reader = new BinaryReader(wavfileStream))
				{
					ChunkId = reader.ReadBytes(4);
					FileSize = reader.ReadUInt32();
					RiffType = reader.ReadBytes(4);
					FmtId = reader.ReadBytes(4);
					FmtSize = reader.ReadUInt32();
					FmtCode = reader.ReadUInt16();
					Channels = reader.ReadUInt16();
					SampleRate = reader.ReadUInt32();
					FmtAvgBps = reader.ReadUInt32();
					FmtBlockAlign = reader.ReadUInt16();
					BitDepth = reader.ReadUInt16();

					if (FmtSize == 18)
					{
						// Read any extra values
						FmtExtraSize = reader.ReadInt16();
						reader.ReadBytes(FmtExtraSize);//Are these bytes needed?
					}

					DataId = reader.ReadBytes(4);
					DataSize = reader.ReadUInt32();
					Data = reader.ReadBytes((int)DataSize);
				}
			}
		}

		public bool AppendFile(WavFile wavFile)
		{
			var flag = true;
			switch (DataSize)
			{
				case 0:
					InitializeFromFile(wavFile);
					break;
				default:
					flag &= AppendNewData(wavFile);
					break;
			}

			flag &= WriteToFile();

			return flag;
		}

		private bool WriteToFile()
		{
			try
			{
				using (var wavfileStream = File.Open(AbsolutPath, FileMode.OpenOrCreate))
				{
					using (var writer = new BinaryWriter(wavfileStream))
					{
						writer.Write(ChunkId);
						writer.Write(FileSize);
						writer.Write(RiffType);
						writer.Write(FmtId);
						writer.Write(FmtSize);
						writer.Write(FmtCode);
						writer.Write(Channels);
						writer.Write(SampleRate);
						writer.Write(FmtAvgBps);
						writer.Write(FmtBlockAlign);
						writer.Write(BitDepth);
						writer.Write(DataId);
						writer.Write(DataSize);
						writer.Write(Data);
					}
				}
			}
			catch (NullReferenceException e)
			{
				Log.Error(Tag, "A Header entry was null in WriteToFile." + e.Message + e.StackTrace);
				return false;
			}

			return true;
		}

		private bool AppendNewData(WavFile wavFile)
		{
			if (!HasSameHeader(wavFile))
			{
				Log.Error("WAVFile", "Not able to append, different headers.");
				return false;
			}

			if (wavFile.DataSize == 0)
			{
				Log.Error(Tag, "trying to add empty wav file.");
				return false;
			}

			var newDataSize = DataSize + wavFile.DataSize;
			var tmp = new byte[newDataSize];

			for (var i = 0; i < newDataSize; i++)
			{
				if (i < DataSize)
					tmp[i] = Data[i];
				else
					tmp[i] = wavFile.Data[i-DataSize];
			}

			Data = tmp;
			DataSize = newDataSize;
			FileSize += wavFile.DataSize;

			return true;
		}

		private bool HasSameHeader(WavFile wavFile)
		{
			return (ChunkId.SequenceEqual(wavFile.ChunkId) &&
					RiffType.SequenceEqual(wavFile.RiffType) &&
					FmtId.SequenceEqual(wavFile.FmtId) &&
					FmtSize == wavFile.FmtSize &&
					FmtCode == wavFile.FmtCode &&
					Channels == wavFile.Channels &&
					SampleRate == wavFile.SampleRate &&
					FmtAvgBps == wavFile.FmtAvgBps &&
					FmtBlockAlign == wavFile.FmtBlockAlign &&
					BitDepth == wavFile.BitDepth &&
					FmtExtraSize == wavFile.FmtExtraSize &&
					DataId.SequenceEqual(wavFile.DataId));
		}

		private void InitializeFromFile(WavFile wavFile)
		{
			ChunkId = wavFile.ChunkId;
			FileSize = wavFile.FileSize;
			RiffType = wavFile.RiffType;
			FmtId = wavFile.FmtId;
			FmtSize = wavFile.FmtSize;
			FmtCode = wavFile.FmtCode;
			Channels = wavFile.Channels;
			SampleRate = wavFile.SampleRate;
			FmtAvgBps = wavFile.FmtAvgBps;
			FmtBlockAlign = wavFile.FmtBlockAlign;
			BitDepth = wavFile.BitDepth;
			FmtExtraSize = wavFile.FmtExtraSize;
			DataId = wavFile.DataId;
			DataSize = wavFile.DataSize;
			Data = wavFile.Data;
		}
	}
}