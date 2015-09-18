
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using SkillStore.Main;

namespace SkillStore.About
{
	[Activity(Label = "@string/AboutTitle", MainLauncher = false, Theme = "@style/SkillStore.Theme.Main.TabbedActivity",
			  Icon = "@drawable/ic_action_about", ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask, 
			  ParentActivity = typeof(TabbedActivity))]
	public class AboutActivity : Activity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.AboutActivityLayout);
			
			if (ActionBar != null)
			{
				ActionBar.Title = Resources.GetString(Resource.String.AboutTitle);
				ActionBar.SetDisplayHomeAsUpEnabled(true);
			}

			var versionText = FindViewById<TextView>(Resource.Id.VersionText);
			versionText.Text = SkillStoreApplication.Version;
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