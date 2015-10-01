
using Android.App;
using Android.OS;
using Android.Preferences;
using Android.Views;
using SkillStore.Main;
using SkillStore.Settings.Views;

namespace SkillStore.Settings
{
	[Activity(Label = "@string/SettingsTitle", Theme = "@android:style/Theme.Holo.Light", Icon = "@drawable/ic_action_settings",
			  ParentActivity = typeof(TabbedActivity))]
	public class SettingsActivity : Activity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.SettingsActivityLayout);

			PreferenceManager.SetDefaultValues(this, Resource.Xml.Preferences, false);
			FragmentManager.BeginTransaction().Replace(Resource.Id.SettingsContent, new SettingsFragment()).Commit();

			if (ActionBar != null)
			{
				ActionBar.Title = Resources.GetString(Resource.String.SettingsTitle);
				ActionBar.SetDisplayHomeAsUpEnabled(true);
			}
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					Finish();
					return true;
				default:
					return base.OnOptionsItemSelected(item);
			}
		}
	}
}