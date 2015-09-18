using System;
using System.Globalization;
using Android.App;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using SkillStore.Service.Recorder;
using SkillStore.Utility;

namespace SkillStore.Main.Views
{
	public class AnalyzerTabFragment : Fragment, SeekBar.IOnSeekBarChangeListener
	{
		private const int MaxProgress = 8;

		private Button _startButton;
		private Button _cancelButton;
		private Button _feedbackButton;
		private TextView _statusText;
		private TextView _resultText;
		private TextView _countdownText;
		private TextView _manualText;
		private TextView _manualText2;
		private LinearLayout _resultViews;
		private ImageView _animationView;
		private ImageView _smartPhoneView;
		private SeekBar _progressBar;
		private CheckBox _dontShowAgainBox;


		private bool IsUiInitialized { get; set; }
		private int ResourceLayoutId { get { return Resource.Layout.AnalyzerTabFragment; } }
		private bool EnableStartButtonFlag { get; set; }
		public bool AnalyzerFailed { private get; set; }
		private bool AnalyzerSuccessful { get; set; }

		public bool ShowsResults { get; private set; }

		public override void OnAttach(Activity activity)
		{
			base.OnAttach(activity);

			((TabbedActivity)activity).AddFragmentToViewPager(this, SkillStoreTabState.Analyzer);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{		
			var view = inflater.Inflate(ResourceLayoutId, container, false);
			return view;
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);

			IsUiInitialized = false;
			AnalyzerFailed = false;
 
			InitializeUi(Activity);
			ShowStartViews(Activity, SkillStoreApplication.IsHelpRequired);
		}

		public override void OnStart()
		{
			base.OnStart();
			if (_startButton != null) _startButton.Click += OnClickStart;
			if (_cancelButton != null) _cancelButton.Click += OnClickCancel;
			if (_feedbackButton != null) _feedbackButton.Click += OnClickFeedback;
			if (_dontShowAgainBox != null) _dontShowAgainBox.CheckedChange += OnCheckedChanged;
		}

		public override void OnStop()
		{
			base.OnStop();

			if (_startButton != null) _startButton.Click -= OnClickStart;
			if (_cancelButton != null) _cancelButton.Click -= OnClickCancel;
			if (_feedbackButton != null) _feedbackButton.Click -= OnClickFeedback;
			if (_dontShowAgainBox != null) _dontShowAgainBox.CheckedChange -= OnCheckedChanged;
		}

		private void OnCheckedChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			var preferences = PreferenceManager.GetDefaultSharedPreferences(SkillStoreApplication.AppContext);
			var prefEditor = preferences.Edit();
			prefEditor.PutBoolean(SettingsKey.ShowAgainPreference, e.IsChecked);
			prefEditor.Commit();
		}

		public void IncrementProgressBar()
		{
			_progressBar.Progress++;
		}

		public void ProgressBarToMax()
		{
			_progressBar.Progress = _progressBar.Max;
		}

		public void EnableStartButton(Activity activity, bool flag)
		{
			EnableStartButtonFlag = flag;
			if (_startButton != null)
			{
				activity.RunOnUiThread(() => _startButton.Enabled = EnableStartButtonFlag);
				
			}
		}

		private void InitializeUi(Activity activity)
		{
			if (IsUiInitialized) return;

			_startButton = activity.FindViewById<Button>(Resource.Id.StartButton);
			_startButton.Enabled = EnableStartButtonFlag;
			_cancelButton = activity.FindViewById<Button>(Resource.Id.CancelButton);
			_feedbackButton = activity.FindViewById<Button>(Resource.Id.FeedbackButton);

			_statusText = activity.FindViewById<TextView>(Resource.Id.StatusText);
			_resultText = activity.FindViewById<TextView>(Resource.Id.Result);
			_animationView = activity.FindViewById<ImageView>(Resource.Id.AnimationView);
			_smartPhoneView = activity.FindViewById<ImageView>(Resource.Id.SmartphoneImage);
			_resultViews = activity.FindViewById<LinearLayout>(Resource.Id.ResultsView);
			_countdownText = activity.FindViewById<TextView>(Resource.Id.CountdownText);
			_manualText = activity.FindViewById<TextView>(Resource.Id.ManualText1);
			_manualText2 = activity.FindViewById<TextView>(Resource.Id.ManualText2);

			_progressBar = activity.FindViewById<SeekBar>(Resource.Id.SeekBar);
			_progressBar.SetOnSeekBarChangeListener(this);
			_progressBar.Touch += OnTouchProgressBar;
			_progressBar.SetThumb(activity.Resources.GetDrawable(Resource.Drawable.thumb_green));
			_progressBar.ProgressDrawable = activity.Resources.GetDrawable(Resource.Drawable.progress_seekbar_green);
			_progressBar.Max = MaxProgress;
			_progressBar.Progress = 0;

			_dontShowAgainBox = activity.FindViewById<CheckBox>(Resource.Id.DontShowAgainBox);
			_dontShowAgainBox.Visibility = ViewStates.Invisible;
			var preferences = PreferenceManager.GetDefaultSharedPreferences(SkillStoreApplication.AppContext);
			_dontShowAgainBox.Checked = preferences.GetBoolean(SettingsKey.ShowAgainPreference, false);

			IsUiInitialized = true;
		}

		public void ShowStartViews(Activity activity, bool isHelpRequired)
		{
			AnalyzerFailed = false;
			HideAllViews(activity);

			activity.RunOnUiThread(() =>
			{
				_startButton.Visibility = ViewStates.Visible;
				_animationView.Visibility = ViewStates.Visible;
				_smartPhoneView.Visibility = ViewStates.Visible;
				_startButton.Enabled = EnableStartButtonFlag;
				_progressBar.Progress = 0;

				if (isHelpRequired)
				{
					_manualText.Text = Resources.GetText(Resource.String.Slide1_Text1);
					_manualText.Visibility = ViewStates.Visible;
					_manualText2.Visibility = ViewStates.Visible;
				}
			});
			StopAnimation(activity);
		}

		public void ShowAnalyzerViews(Activity activity)
		{
			HideAllViews(activity);

			activity.RunOnUiThread(() =>
			{
				_progressBar.Visibility = ViewStates.Visible;
				_statusText.Visibility = ViewStates.Visible;
				_animationView.Visibility = ViewStates.Visible;
				_smartPhoneView.Visibility = ViewStates.Visible;
			});
		}

		public void ShowCountdownViews(Activity activity, bool isHelpRequired)
		{
			ShowAnalyzerViews(activity);
			activity.RunOnUiThread(() =>
			{
				_countdownText.Visibility = ViewStates.Visible;
			});
			StopAnimation(activity);
		}

		public void ShowResultViews(Activity activity, string result, bool isHelpRequired)
		{
			if (Activity == null) return;

			AnalyzerSuccessful = true;

			HideAllViews(activity);
			ChangeStatusText(activity, Resources.GetString(Resource.String.AnalysisSuccessful));
			ChangeResultText(activity, result);
			StopAnimation(activity);

			activity.RunOnUiThread(() =>
			{
				ChangeSeekBarColor(_progressBar);
				_cancelButton.Visibility = ViewStates.Visible;
				_feedbackButton.Visibility = ViewStates.Visible; //TODO: Build other feedback button behavior when FAILED status.
				_progressBar.Visibility = ViewStates.Visible;
				_resultViews.Visibility = ViewStates.Visible;
				ProgressBarToMax();

				if (isHelpRequired)
				{
					_dontShowAgainBox.Visibility = ViewStates.Visible;
					_manualText.Text = Resources.GetText(Resource.String.Slide3_Text2);
					_manualText.Visibility = ViewStates.Visible;
				}
			});

			ShowsResults = true;
		}

		private void HideAllViews(Activity activity)
		{
			activity.RunOnUiThread(() =>
			{
				InitializeUi(activity);
				_cancelButton.Visibility = ViewStates.Invisible;
				_feedbackButton.Visibility = ViewStates.Invisible;
				_progressBar.Visibility = ViewStates.Invisible;
				_statusText.Visibility = ViewStates.Invisible;
				_manualText.Visibility = ViewStates.Invisible;
				_manualText2.Visibility = ViewStates.Invisible;
				_startButton.Visibility = ViewStates.Invisible;
				_resultViews.Visibility = ViewStates.Invisible;
				_countdownText.Visibility = ViewStates.Invisible;
				_animationView.Visibility = ViewStates.Invisible;
				_smartPhoneView.Visibility = ViewStates.Invisible;
				_dontShowAgainBox.Visibility = ViewStates.Invisible;
			});

			ShowsResults = false;
		}

		public void ChangeStatusText(Activity activity, string msg)
		{
			activity.RunOnUiThread(() => { if (_statusText != null) _statusText.Text = msg; });
		}

		private void ChangeResultText(Activity activity, string msg)
		{
			activity.RunOnUiThread(() => { if (_resultText != null) _resultText.Text = msg; });
		}

		public void UpdateCountdownView(Activity activity, AudioRecorderEventArgs e)
		{
			activity.RunOnUiThread(() =>
			{
				if (e.IsCounting)
				{
					_countdownText.Text = e.Counter.ToString(CultureInfo.InvariantCulture);
				}
			});
		}

		public void StartAnimation(Activity activity)
		{
			activity.RunOnUiThread(() =>
			{
				var animation = AnimationUtils.LoadAnimation(Activity, Resource.Animation.ShakeAnimation);
				_animationView.StartAnimation(animation);
				_animationView.Visibility = ViewStates.Visible;
			});
		}

		public void StopAnimation(Activity activity)
		{
			activity.RunOnUiThread(() => _animationView.ClearAnimation());
		}

		private void OnClickStart(object sender, EventArgs e)
		{
			ShowAnalyzerViews(Activity);
			((TabbedActivity)Activity).AnalyzerStarted();
		}

		private void OnClickCancel(object sender, EventArgs e)
		{
			ShowStartViews(Activity, SkillStoreApplication.IsHelpRequired);
			((TabbedActivity) Activity).ResetAnalyzer();
			ShowsResults = false;
		}

		private void OnClickFeedback(object sender, EventArgs e)
		{
			((TabbedActivity)Activity).StartFeedbackActivity();
		}

		private void OnTouchProgressBar(object sender, View.TouchEventArgs e)
		{
			//just do nothing to lock disable the user input on the progress bar. 
			//Enabled property does not work due to graying of view.
		}

		public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
		{
			if (fromUser) return;

			seekBar.Progress = progress;
			ChangeSeekBarColor(seekBar);
		}

		public void OnStartTrackingTouch(SeekBar seekBar) { }

		public void OnStopTrackingTouch(SeekBar seekBar) { }

		private void ChangeSeekBarColor(SeekBar seekBar)
		{
			if (AnalyzerFailed)
			{
				seekBar.SetThumb(Resources.GetDrawable(Resource.Drawable.thumb_red));
				seekBar.ProgressDrawable = Resources.GetDrawable(Resource.Drawable.progress_seekbar_red);
				return;
			}

			ChangeSeekBarColorAnalyzerNotFailed(seekBar);
		}

		private void ChangeSeekBarColorAnalyzerNotFailed(SeekBar seekBar)
		{
			if ((seekBar.Progress == MaxProgress && AnalyzerSuccessful) || seekBar.Progress == 0)
			{
				seekBar.SetThumb(Resources.GetDrawable(Resource.Drawable.thumb_green));
				seekBar.ProgressDrawable = Resources.GetDrawable(Resource.Drawable.progress_seekbar_green);
				return;
			}

			seekBar.SetThumb(Resources.GetDrawable(Resource.Drawable.thumb_yellow));
			seekBar.ProgressDrawable = Resources.GetDrawable(Resource.Drawable.progress_seekbar_yellow);
		}
	}
}