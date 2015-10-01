using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using SkillStore.Utility;

namespace SkillStore.Main.Views
{
	public class HistoryTabFragment : ListFragment, AdapterView.IOnItemLongClickListener
	{

		private int ResourceLayoutId { get { return Resource.Layout.HistoryTabFragment; } }

		private List<DataObject> Items
		{
			set
			{
				var items = value;
				items.Reverse();
				ListAdapter = new HistoryItemAdapter(Activity, items);
			}
		}

		public void SetItems(Activity activity, List<DataObject> data)
		{
			activity.RunOnUiThread(() => { Items = data; });
		}

		public override void OnAttach(Activity activity)
		{
			base.OnAttach(activity);
			((TabbedActivity)Activity).AddFragmentToViewPager(this, SkillStoreTabState.History);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = inflater.Inflate(ResourceLayoutId, container, false);
			return view;
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);

			RegisterForContextMenu(ListView);
		}

		public void ShowEmptyHistory(Activity activity)
		{
			activity.RunOnUiThread(() =>
			{
				var item = new DataObject(string.Empty);
				var items = new List<DataObject> { item };
				Items = items;
			});
			
		}

		public override void OnListItemClick(ListView l, View v, int position, long id)
		{
			base.OnListItemClick(l, v, position, id);

			var adapter = ListAdapter as HistoryItemAdapter;
			if (adapter == null) return;
			if (adapter.IsListEmpty) return;

			var item = adapter[position];
			if (item != null) ((TabbedActivity)Activity).StartFeedbackActivity(item);
		}

		public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
		{
			throw new NotImplementedException();
		}

		public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
		{
			base.OnCreateContextMenu(menu, v, menuInfo);

			if (v.Id != ListView.Id) return;

			var menuItems = Resources.GetStringArray(Resource.Array.HistoryItemContextMenu);
			for (var i = 0; i < menuItems.Length; i++)
			{
				menu.Add(Menu.None, i, Menu.None, menuItems[i]);
			}
		}

		public override bool OnContextItemSelected(IMenuItem item)
		{
			var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
			var menuItemIndex = item.ItemId;
			var listPosition = info.Position;

			var adapter = ListAdapter as HistoryItemAdapter;
			if (adapter == null) return false;
			if (adapter.IsListEmpty) return false;
			var listItem = adapter[listPosition];
			ResolveContextMenuItemAction(menuItemIndex, listItem);

			return true;
		}

		private void ResolveContextMenuItemAction(int menuItemIndex, DataObject listItem)
		{
			switch ((HistoryItemContextMenuItemType) menuItemIndex)
			{
				case HistoryItemContextMenuItemType.Edit:
					((TabbedActivity) Activity).StartFeedbackActivity(listItem);
					break;
				case HistoryItemContextMenuItemType.Delete:
					((TabbedActivity) Activity).DeleteData(listItem);
					break;
				case HistoryItemContextMenuItemType.DeleteHistory:
					((TabbedActivity) Activity).DeleteAllData();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}