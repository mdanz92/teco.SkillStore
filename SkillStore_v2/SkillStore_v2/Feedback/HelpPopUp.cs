using System;
using Android.Content;
using Android.Graphics;
using Android.Text.Method;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace SkillStore.Feedback
{
	public class HelpPopUp
	{

		private readonly Context _context;
		private PopupWindow _window;

		private TextView _helpTextView;
		private RelativeLayout _upImageView;
		private RelativeLayout _downImageView;
		private View _view;

		private Point _displaySize;

		private Point RootSize
		{
			get
			{
				var rootSize = new Point();
				_view.Measure(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
				rootSize.Y = _view.MeasuredHeight;
				rootSize.X = _view.MeasuredWidth;
				return rootSize;
			}
		}

		public HelpPopUp(Context context, string msg = "", int viewResource = Resource.Layout.HelpPopUpLayout)
		{
			_context = context;
			InitializeUi(viewResource, msg);
		}

		private void InitializeUi(int viewResource, string msg)
		{
			_window = new PopupWindow(_context);

			var displayMetrics = _context.Resources.DisplayMetrics;
			_displaySize = new Point();
			_displaySize.X = displayMetrics.WidthPixels;
			_displaySize.Y = displayMetrics.HeightPixels;


			SetContentView(viewResource);

			_upImageView = _view.FindViewById<RelativeLayout>(Resource.Id.ArrowUp);
			_downImageView = _view.FindViewById<RelativeLayout>(Resource.Id.ArrowDown);

			_helpTextView = _view.FindViewById<TextView>(Resource.Id.Message);
			_helpTextView.MovementMethod = ScrollingMovementMethod.Instance;
			_helpTextView.Selected = true;
			_helpTextView.Text = msg;
		}


		public void Show(View anchor)
		{
			PreShow();

			var anchorRect = CalculateAnchorRect(anchor);
			var arrowOnTop = anchorRect.Top > (_displaySize.Y / 2);
			var yPos = CalculateYPos(anchorRect, arrowOnTop);

			SetHelpTextMaxHeight(arrowOnTop, anchorRect, yPos);
			HideArrow(arrowOnTop);

			//var showArrow = GetShowArrow(arrowOnTop);
			//var arrowWidth = showArrow.MeasuredWidth;
			//var requestedX = anchorRect.CenterX();
			var xPos = CalculateXPos(anchorRect);
			//var param = (ViewGroup.MarginLayoutParams) showArrow.LayoutParameters;
			//param.LeftMargin = (requestedX - xPos) - (arrowWidth/2);
			//showArrow.LayoutParameters = param;

			_window.ShowAtLocation(anchor, GravityFlags.NoGravity, xPos, yPos);
			_view.Animation = AnimationUtils.LoadAnimation(_context, Resource.Animation.HelpPopUpFloatingAnimation);
		}

		private void SetHelpTextMaxHeight(bool arrowOnTop, Rect anchorRect, int yPos)
		{
			if (arrowOnTop)
			{
				_helpTextView.SetMaxHeight(_displaySize.Y - yPos);
			}
			else
			{
				_helpTextView.SetMaxHeight(anchorRect.Top - anchorRect.Height());
			}
		}

		private static Rect CalculateAnchorRect(View anchor)
		{
			var location = new int[2];
			anchor.GetLocationOnScreen(location);

			return new Rect(location[0], location[1], location[0] + anchor.Width, location[1] + anchor.Height);

		}

		private int CalculateYPos(Rect anchorRect, bool arrowOnTop)
		{
			var yPos = anchorRect.Top - RootSize.Y;
			if (!arrowOnTop)
			{
				yPos = anchorRect.Bottom;
			}
			return yPos;
		}

		private void HideArrow(bool arrowOnTop)
		{
			var hideArrow = arrowOnTop ? _upImageView : _downImageView;
			hideArrow.Visibility = ViewStates.Gone;
		}

		private int CalculateXPos(Rect anchorRect)
		{
			int xPos;
			if (anchorRect.Left + RootSize.X > _displaySize.X)
			{
				xPos = _displaySize.X - RootSize.X;
			}

			else if (anchorRect.Left - (RootSize.X / 2) < 0)
			{
				xPos = anchorRect.Left;
			}
			else
			{
				xPos = anchorRect.CenterX() - (RootSize.X / 2);
			}
			return xPos;
		}

		private void PreShow()
		{
			if (_view == null)
			{
				throw new InvalidOperationException("View undefined");
			}

			_window.SetBackgroundDrawable(_context.Resources.GetDrawable(Android.Resource.Color.Transparent));
			_window.Width = ViewGroup.LayoutParams.WrapContent;
			_window.Height = ViewGroup.LayoutParams.WrapContent;
			_window.Touchable = true;
			_window.Focusable = true;
			_window.OutsideTouchable = true;

			_window.ContentView = _view;
		}

		private void SetContentView(int layoutResId)
		{
			var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
			_view = inflater.Inflate(layoutResId, null);
			_window.ContentView = _view;
		}
	}
}