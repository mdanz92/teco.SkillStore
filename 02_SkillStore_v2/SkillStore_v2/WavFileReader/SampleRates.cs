using System;
using System.Globalization;

namespace SkillStore.WavFileReader
{
	public static class SampleRates
	{
		public const int VeryHigh = 44100;
		public const int High = 22050;
		public const int Medium = 11025;
		public const int Low = 8000;
		public const int NumberSampleRates = 4;

		public static int GetValueAt(int key)
		{
			switch (key)
			{
				case 0:
					return VeryHigh;
				case 1:
					return High;
				case 2:
					return Medium;
				case 3:
					return Low;
				default:
					throw new ArgumentOutOfRangeException(key.ToString(CultureInfo.InvariantCulture));
			}
		}
	}
}