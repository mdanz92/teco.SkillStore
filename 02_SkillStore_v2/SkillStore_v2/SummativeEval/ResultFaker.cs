using System;
using System.Collections.Generic;


namespace SkillStore.SummativeEval
{
	public static class ResultFaker
	{
		private static readonly List<string> Results = new List<string>() {"32", "8", "18", "23", "29", "31", "34", "35", "40"};
		private static int _count;

		public static string GetCurrentResult()
		{
			return _count > 0 ? Results[_count-1] : Results[_count];
		}

		public static string GetNextResult()
		{
			if (_count > 8)
				_count = 0;

			if (_count == 0)
			{
				_count++;
				return Results[new Random().Next(0, 8)];
			}

			if (_count != 3 && _count != 6) return Results[_count++];

			if (new Random().Next(100) >= 35) return Results[_count++];

			_count++;
			return Results[new Random().Next(0, 8)];
		}

	}
}