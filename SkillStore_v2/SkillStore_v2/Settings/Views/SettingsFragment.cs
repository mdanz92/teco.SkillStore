
using Android.OS;
using Android.Preferences;

namespace SkillStore.Settings.Views
{
	public class SettingsFragment : PreferenceFragment
	{
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			AddPreferencesFromResource(Resource.Xml.Preferences);
		}
	}
}