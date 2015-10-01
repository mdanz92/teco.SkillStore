
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Net;
using Android.Views;
using Android.Widget;
using SkillStore.Utility;

namespace SkillStore.Main.Views
{
	public class HistoryItemAdapter : BaseAdapter<DataObject>
	{
		private readonly List<DataObject> _items;
		private readonly Context _context;

		public bool IsListEmpty { get { return _items.Count == 1 && _items.First() != null && _items.First().Id == string.Empty; } }
		public override int Count { get { return _items.Count; } }
		public override DataObject this[int position] { get { return _items[position]; } }
		public override long GetItemId(int position) { return position; }

		public HistoryItemAdapter(Context context, List<DataObject> items)
		{
			_context = context;
			_items = items;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			if (IsListEmpty) return InflateEmptyHistoryItem(parent);

			var item = _items[position];
			if (item == null) return InflateReadFailedHistoryItem(parent);

			var view =  InflateHistoryItem(parent);
			view.FindViewById<TextView>(Resource.Id.Result).Text = item.Class;
			view.FindViewById<TextView>(Resource.Id.Date).Text = item.Date;
			view.FindViewById<TextView>(Resource.Id.Tags).Text = item.Tags;
			
			if (string.IsNullOrEmpty(item.PicturePath))
				view.FindViewById<ImageView>(Resource.Id.Preview).SetImageResource(Resource.Drawable.PreviewSample);
			else
				view.FindViewById<ImageView>(Resource.Id.Preview).SetImageURI(Uri.Parse(item.PicturePath));
			return view;
		}

		private View InflateReadFailedHistoryItem(ViewGroup parent)
		{
			var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
			return inflater.Inflate(Resource.Layout.ReadFailedHistoryItem, parent, false);
		}

		private View InflateEmptyHistoryItem(ViewGroup parent)
		{
			var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
			return inflater.Inflate(Resource.Layout.EmptyHistoryItem, parent, false);
		}

		private View InflateHistoryItem(ViewGroup parent)
		{
			var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
			return inflater.Inflate(Resource.Layout.HistoryItem, parent, false);
		}

		
	}
}