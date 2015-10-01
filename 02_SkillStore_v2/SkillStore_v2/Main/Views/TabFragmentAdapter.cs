using System;
using System.Collections.Generic;
using Android.App;
using Android.Runtime;
using Android.Support.V13.App;
using SkillStore.Utility;

namespace SkillStore.Main.Views
{
	public class TabFragmentAdapter : FragmentPagerAdapter
	{
		public Dictionary<SkillStoreTabState, Fragment> Tabs { get; private set; }
		public override int Count
		{
			get { return 2; }
		}

		public TabFragmentAdapter(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
			Initialization();
		}

		public TabFragmentAdapter(FragmentManager fm)
			: base(fm)
		{
			Initialization();
		}

		private void Initialization()
		{
			Tabs = new Dictionary<SkillStoreTabState, Fragment>();
			Tabs.Add(SkillStoreTabState.Analyzer, new AnalyzerTabFragment());
			Tabs.Add(SkillStoreTabState.History, new HistoryTabFragment());
		}

		public override Fragment GetItem(int position)
		{
			return Tabs.ContainsKey((SkillStoreTabState)position) ? Tabs[(SkillStoreTabState)position] : null;
		}
	}
}