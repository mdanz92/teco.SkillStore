
using System.Collections.Generic;
using System.Linq;

namespace SkillStore.SummativeEval
{
	public class EggMapper
	{
		private static readonly Dictionary<string, string> Map = new Dictionary<string, string>()
		{
			{"7", "7"}, {"12", "12"}, {"15", "15"}, {"17", "17"}, {"18", "18"}, {"19", "19"}, {"22", "22"}, {"24", "24"}, {"25", "25"}, 
			{"29", "29"}, {"30", "30"}, {"32", "32"}, {"33", "33"}, {"35", "35"}, {"36", "36"}, {"37", "37"}, {"40", "40"}, {"41", "1"}, 
			{"42", "2"}, {"44", "3"}, {"45", "4"}, {"46", "5"}, {"47", "6"}, {"49", "8"}, {"50", "9"}, {"53", "10"}, {"54", "11"}, {"55", "13"}, 
			{"56", "14"}, {"57", "16"}, {"59", "20"}, {"60", "21"}, {"61", "23"}, {"62", "26"}, {"63", "27"}, {"64", "28"}, {"65", "31"}, 
			{"66", "34"}, {"67", "38"}, {"68", "39"}
		};

		public static string GetMappedName(string eggName)
		{
			string mapping;
			return Map.TryGetValue(eggName, out mapping) ? mapping : null;
		}

		public static string GetRealName(string eggName)
		{
			return Map.FirstOrDefault(x => x.Value == eggName).Key;
		}

	}
}