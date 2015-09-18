using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using SkillStore.About;
using SkillStore.Error;
using SkillStore.Feedback;
using SkillStore.Main.Views;
using SkillStore.Service;
using SkillStore.Service.Communication;
using SkillStore.Service.Recorder;
using SkillStore.Service.Storage;
using SkillStore.Settings;
using SkillStore.Utility;

namespace SkillStore.Main
{
	[Activity(Label = "@string/ApplicationName", MainLauncher = false, Theme = "@style/SkillStore.Theme.Main.TabbedActivity", 
			  ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
	public class TabbedActivity : Activity, ISkillStoreServiceConnectable, ViewPager.IOnPageChangeListener, IErrorPopUpDismissListener
	{

		#region Attributes

		private const string Tag = "TabbedActivity";
		private readonly object _fragmentLock = new object();

		private SkillStoreTabState _currentTabState;
		private ViewPager _viewPager;
		private Fragment _currentFragment;
		private RelativeLayout _overlay;
		private TextView _overlayStatusText;

		private PowerManager.WakeLock _wakeLock;

		#endregion


		#region Properties

		public bool ServerCommBound { get; set; }
		public bool StorageBound { get; set; }
		public bool RecorderBound { get; set; }

		public bool AllServicesBound { get { return ServerCommBound && StorageBound && RecorderBound; } }

		public ServerConnection ServerConnection { get; set; }
		public ServerCommService ServerCommService { get; set; }
		public new StorageService StorageService { get; set; }
		public StorageConnection StorageConnection { get; set; }
		public AudioRecorderConnection AudioRecorderConnection { get; set; }
		public AudioRecorderService AudioRecorderService { get; set; }
		private bool IsStorageCommHandlerConnected { get; set; }
		private bool IsServerCommHandlerConnected { get; set; }
		private bool IsAudioRecorderCommHandlerConnected { get; set; }

		private bool IsHistoryReloadNeeded { get; set; }
		private bool IsAnalyzerResetNeeded { get; set; }
		private bool IsAnalyzing { get; set; }
		private bool IsInAnalyzer { get { return _currentTabState == SkillStoreTabState.Analyzer; } }
		private DataObject CurrentData { get; set; }

		private bool AnalyzerShowsResults
		{
			get //TODO refactor this
			{
				if (_currentFragment == null) return false;
				lock (_fragmentLock)
				{
					if (_currentFragment.GetType() != typeof (AnalyzerTabFragment)) return false;
					if (!((AnalyzerTabFragment) _currentFragment).ShowsResults) return false;
				}
				return true;
			}
		}

		#endregion

		#region Activity life cycle

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			InitializeUiLayout();
			InitializeTabs();
			InitializeViewPager();
		}
		
		protected override void OnStart()
		{
			base.OnStart();

			if (!AllServicesBound)
			{
				BindServices();
			}
			ConnectCommunicationHandler();
			ErrorPopUpFactory.DismissHandler += OnErrorPopUpDismissEvent;
			if (IsAnalyzerResetNeeded)
			{			
				ResetAnalyzer();
			}
			if (IsHistoryReloadNeeded)
			{
				ReloadHistory();
			}
		}
		
		protected override void OnResume()
		{
			base.OnResume();

			TryEnableStartButton();
		}

		protected override void OnPause()
		{
			UnbindeServices();
			ErrorPopUpFactory.DismissHandler -= OnErrorPopUpDismissEvent;
			base.OnPause();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			switch ((ActivityRequestCode)requestCode)
			{
				case ActivityRequestCode.SettingsActivityFinished:
					HandleSettingsActivityFinished(resultCode);
					break;
				case ActivityRequestCode.AboutActivityFinished:
					HandleAboutActivityFinished(resultCode);
					break;
				case ActivityRequestCode.HelpActivityFinished:
					HandleHelpActivityFinished(resultCode);
					break;
				case ActivityRequestCode.FeedbackActivityFinished:
					HandleFeedbackActivityFinished(resultCode);
					break;
				case ActivityRequestCode.PickImageActionFinished:
					break;
				default:
					throw new ArgumentOutOfRangeException("requestCode");
			}
		}

		public void AnalyzerStarted()
		{
			ToggleKeepScreenOn(true);

			if (AudioRecorderService != null)
			{				
				AudioRecorderService.StartRecorder(GenerateNewDataObject());
				IsAnalyzing = true;
			}
			else
			{
				Log.Error(Tag, "AudioRecorderService was null on analyzer start.");
			}
		}

		private void ToggleKeepScreenOn(bool flag)
		{
			if (flag)
			{
				var powerManager = (PowerManager) GetSystemService(PowerService);
				_wakeLock = powerManager.NewWakeLock(WakeLockFlags.ScreenBright, Tag);
				_wakeLock.Acquire();
				return;
			}
			try
			{
				if (_wakeLock != null) _wakeLock.Release();
			}
			catch (Java.Lang.RuntimeException e)
			{
				Log.Error(Tag, "ToggleKeepScreenOn: Exception on lock release." + e.Message + e.StackTrace);
			}
		}

		public void DeleteData(DataObject data)
		{
			StorageService.DeleteData(data);
		}

		public void DeleteAllData()
		{
			StorageService.DeleteAllData();
		} 

		private string GenerateNewDataObject()
		{
			var idStr = DateTime.UtcNow.ToString(Resources.GetString(Resource.String.DateCulture));
			CurrentData = new DataObject(idStr, Build.Model);
			return CurrentData.Id;
		}

		public void ResetAnalyzer()
		{
			UpdateTabSelection(SkillStoreTabState.Analyzer);
			StopServices();
			
			if (IsInAnalyzer && _currentFragment != null)
			{
				lock (_fragmentLock)
				{
					((AnalyzerTabFragment)_currentFragment).ShowStartViews(this, SkillStoreApplication.IsHelpRequired);
				}
			}

			GenerateNewDataObject();
			IsAnalyzing = false;
			IsAnalyzerResetNeeded = false;

			ToggleKeepScreenOn(false);
			TryEnableStartButton();
		}

		private void ReloadHistory()
		{
			if (IsInAnalyzer) return;
			StorageService.LoadAllData();
			IsHistoryReloadNeeded = false;
		}

		private void ResetFragments()
		{
			if (IsInAnalyzer) IsAnalyzerResetNeeded = true;
			else IsHistoryReloadNeeded = true;
		}

		private void TryEnableStartButton()
		{
			if (!AllServicesBound || !IsInAnalyzer || _currentFragment == null) return;

			lock (_fragmentLock)
			{
				((AnalyzerTabFragment) _currentFragment).EnableStartButton(this, true);
			}
		}

		//private void DisableStartButton()
		//{
		//	if (!IsInAnalyzer || _currentFragment == null) return;

		//	lock (_fragmentLock)
		//	{
		//		((AnalyzerTabFragment)_currentFragment).EnableStartButton(this, false);
		//	}
		//}

		private void HandleSettingsActivityFinished(Result resultCode)
		{
			if (resultCode == Result.Ok)
				ResetFragments();
			else
				Log.Debug(Tag, "@SettingsActivity was canceled by user or due to some error.");
		}

		private void HandleAboutActivityFinished(Result resultCode)
		{
			if (resultCode == Result.Ok)
				ResetFragments();
			else
				Log.Debug(Tag, "@AboutActivity was canceled by user or due to some error.");
		}

		private void HandleHelpActivityFinished(Result resultCode)
		{
			if (resultCode == Result.Ok)
			{
				ResetFragments();
			}
			else
				Log.Debug(Tag, "@HelpActivity was canceled by user or due to some error.");
		}

		private void HandleFeedbackActivityFinished(Result resultCode)
		{
			ConnectCommunicationHandler();

			if (resultCode == Result.Ok)
			{
				ResetFragments();
				Toast.MakeText(this, Resources.GetString(Resource.String.SendingSuccessful), ToastLength.Long).Show();
			}
			else if (resultCode == Result.Canceled)
			{
				if (!IsInAnalyzer) IsHistoryReloadNeeded = true;
			}
			else
				Log.Debug(Tag, "@FeedbackActivity was canceled by user or due to some error.");
		}

		#endregion

		#region Layout

		private void InitializeUiLayout()
		{
			RequestWindowFeature(WindowFeatures.ActionBar);
			ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
			SetContentView(Resource.Layout.TabbedLayout);

			_overlay = FindViewById<RelativeLayout>(Resource.Id.Overlay);
			_overlayStatusText = FindViewById<TextView>(Resource.Id.OverlayStatusText);
		}

		private void InitializeTabs()
		{
			var analyzerTab = ActionBar.NewTab();
			analyzerTab.SetText(Resources.GetString(Resource.String.AnalyzerTitle));
			analyzerTab.TabSelected += OnSelectAnalyzer;
			analyzerTab.TabReselected += OnReselectAnalyzer;
			analyzerTab.TabUnselected += OnUnselectAnalyzer;
			ActionBar.AddTab(analyzerTab);

			var historyTab = ActionBar.NewTab();
			historyTab.SetText(Resources.GetString(Resource.String.HistoryTitle));
			historyTab.TabSelected += OnSelectHistory;
			historyTab.TabReselected += OnReselectHistoy;
			historyTab.TabUnselected += OnUnselectHistory;
			ActionBar.AddTab(historyTab);

			_viewPager = FindViewById<ViewPager>(Resource.Id.ViewPager);

			_currentTabState = SkillStoreTabState.Analyzer;
		}

		private void InitializeViewPager()
		{
			_viewPager.Adapter = new TabFragmentAdapter(FragmentManager);
			_viewPager.SetOnPageChangeListener(this);

			UpdateTabSelection(_currentTabState);
		}

		private void ShowOverlay()
		{
			RunOnUiThread(() =>
			{
				if (_overlay != null && _overlay.Visibility != ViewStates.Visible) _overlay.Visibility = ViewStates.Visible;
			});
		}

		private void HideOverlay()
		{
			RunOnUiThread(() =>
			{
				if (_overlay != null && _overlay.Visibility != ViewStates.Gone) _overlay.Visibility = ViewStates.Gone;
			});
		}

		private void ChangeOverlayStatusText(string msg)
		{
			RunOnUiThread(() => { if (_overlayStatusText != null) _overlayStatusText.Text = msg; });
		}

		public void OnErrorPopUpDismissEvent(object s, EventArgs e)
		{
			HideOverlay();

			if (IsInAnalyzer) ResetAnalyzer(); //TODO what happens in history fragment?
		}

		private void ShowErrorViews(Activity activity, ErrorType error)
		{
			ShowOverlay();
			ErrorPopUpFactory.MakeErrorPopUp(activity, error);

			lock (_fragmentLock)
			{
				if (!IsInAnalyzer) return;
				((AnalyzerTabFragment)_currentFragment).ShowAnalyzerViews(activity);
				((AnalyzerTabFragment)_currentFragment).StopAnimation(activity);
				((AnalyzerTabFragment)_currentFragment).ProgressBarToMax();
			}
			SetErrorStatus(activity, error);
		}

		private void SetErrorStatus(Activity activity, ErrorType error)
		{
			if (!IsInAnalyzer || _currentFragment == null) return;

			lock (_fragmentLock)
			{
				((AnalyzerTabFragment)_currentFragment).AnalyzerFailed = true;
				((AnalyzerTabFragment)_currentFragment).ChangeStatusText(activity, ErrorPopUpFactory.ResolveErrorTitle(error));
			}
		}

		private void OnSelectAnalyzer(object sender, ActionBar.TabEventArgs e)
		{
			UpdateTabSelection(SkillStoreTabState.Analyzer);

			if (AnalyzerShowsResults)
			{
				if (SkillStoreApplication.StoredData != null) CurrentData = SkillStoreApplication.StoredData;
				lock (_fragmentLock)
				{
					((AnalyzerTabFragment) _currentFragment).ShowResultViews(this, CurrentData.Class, SkillStoreApplication.IsHelpRequired);
				}
			}
			else
			{
				ResetAnalyzer();
			}
		}

		private void OnReselectAnalyzer(object sender, ActionBar.TabEventArgs e)
		{
			//TODO refresh
		}

		private void OnUnselectAnalyzer(object sender, ActionBar.TabEventArgs e)
		{
			if (AnalyzerShowsResults)
			{
				SkillStoreApplication.StoredData = CurrentData;
			}
			StopServices();
		}

		private void OnSelectHistory(object sender, ActionBar.TabEventArgs e)
		{
			UpdateTabSelection(SkillStoreTabState.History);
			ReloadHistory();
		}

		private void OnReselectHistoy(object sender, ActionBar.TabEventArgs e) { }

		private void OnUnselectHistory(object sender, ActionBar.TabEventArgs e)
		{
			StopServices();
		}

		private void UpdateTabSelection(SkillStoreTabState state)
		{
			_currentTabState = _currentTabState != state ? state: _currentTabState;
			
			if (_viewPager != null)
			{
				_viewPager.SetCurrentItem((int)_currentTabState, true);

				lock (_fragmentLock)
				{
					_currentFragment = ((TabFragmentAdapter)_viewPager.Adapter).GetItem((int)_currentTabState);
				}
			}

			if (ActionBar != null) ActionBar.SetSelectedNavigationItem((int)_currentTabState);
		}

		public void OnPageScrollStateChanged(int state) { }

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels) { }

		public void OnPageSelected(int position)
		{
			if (AnalyzerShowsResults)
			{
				SkillStoreApplication.StoredData = CurrentData;
			}
			UpdateTabSelection((SkillStoreTabState)position);
		}

		public void AddFragmentToViewPager(Fragment fragment, SkillStoreTabState state)
		{
			if (_viewPager != null)
				((TabFragmentAdapter)_viewPager.Adapter).Tabs[state] = fragment;
		}

		#endregion

		#region Menu navigation

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			var inflater = MenuInflater;
			inflater.Inflate(Resource.Menu.tabbed_activity_actions, menu);

			return base.OnCreateOptionsMenu(menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.ActionSettings:
					StartSettingsActivity();
					return true;
				case Resource.Id.ActionAbout:
					StartAboutActivity();
					return true;
				case Resource.Id.ActionHelp:
					StartHelpActivity();
					return true;
				default:
					return base.OnOptionsItemSelected(item);
			}
		}

		private void StartSettingsActivity()
		{
			StartActivityForResult(typeof(SettingsActivity), (int)ActivityRequestCode.SettingsActivityFinished);
		}

		private void StartAboutActivity()
		{
			StartActivityForResult(typeof(AboutActivity), (int)ActivityRequestCode.AboutActivityFinished);
		}

		private void StartHelpActivity()
		{
			//StartActivityForResult(typeof(HelpActivity), (int)ActivityRequestCode.HelpActivityFinished);
			//HelpExecutedOnce = true;

			var preferences = PreferenceManager.GetDefaultSharedPreferences(this);
			var editor = preferences.Edit();
			editor.PutBoolean("ShowAgainPreference", false);
			editor.Commit();

			if (IsInAnalyzer) ResetAnalyzer();
		}

		public void StartFeedbackActivity(DataObject data = null)
		{
			if (data != null) CurrentData = data;

			if (CurrentData == null)
			{
				Log.Error(Tag, "Tried to start FeedbackActivity but CurrentData was null");
				return;
			}

			var feedbackIntent = new Intent(this, typeof(FeedbackActivity));
			feedbackIntent.PutExtra("Id", CurrentData.Id);
			StartActivityForResult(feedbackIntent, (int)ActivityRequestCode.FeedbackActivityFinished);
		}

		public override void OnBackPressed()
		{
			if (IsInAnalyzer && IsAnalyzing)
			{
				ResetAnalyzer();
			}
			else if (!IsInAnalyzer)
			{
				UpdateTabSelection(SkillStoreTabState.Analyzer);
			}
			else
			{
				Finish();
			}
			//base.OnBackPressed();
		}

		#endregion

		#region Service binding

		private void BindServices()
		{
			if (ServerConnection == null)
			{
				var intent = new Intent("Skillstore.Service.Communication.ServerCommService");
				ServerConnection = new ServerConnection(this);
				BindService(intent, ServerConnection, Bind.AutoCreate);
			}

			if (StorageConnection == null)
			{
				var intent = new Intent("Skillstore.Service.Storage.StorageService");
				StorageConnection = new StorageConnection(this);
				BindService(intent, StorageConnection, Bind.AutoCreate);
			}

			if (AudioRecorderConnection == null) 
			{ 
				var intent = new Intent("Skillstore.Service.AudioRecorder.AudioRecorderService");
				AudioRecorderConnection = new AudioRecorderConnection(this);
				BindService(intent, AudioRecorderConnection, Bind.AutoCreate);
			}
		}

		private void ConnectCommunicationHandler()
		{
			if (AudioRecorderService != null && !IsAudioRecorderCommHandlerConnected)
			{
				AudioRecorderService.Communication += OnAudioRecorderCommChanged;
				IsAudioRecorderCommHandlerConnected = true;
			}
			if (ServerCommService != null && !IsServerCommHandlerConnected)
			{
				ServerCommService.Communication += OnServerCommChanged;
				IsServerCommHandlerConnected = true;
			}
			if (StorageService != null && !IsStorageCommHandlerConnected)
			{
				StorageService.Communication += OnStorageCommChanged;
				IsStorageCommHandlerConnected = true;
			}
		}

		public void OnServiceBound(ServerCommBinder binder)
		{
			ServerCommService = binder.Service;

			if (ServerCommService == null)
			{
				Log.Error(Tag, "ServerCommService was null!");
				return;
			}


			if (!IsServerCommHandlerConnected)
			{
				ServerCommService.Communication += OnServerCommChanged;
				IsServerCommHandlerConnected = true;
			}
			TryEnableStartButton();
		}

		public void OnServiceBound(StorageBinder binder)
		{
			StorageService = binder.Service;

			if (StorageService == null)
			{
				Log.Error(Tag, "StorageService was null!");
				return;
			}

			if (!IsStorageCommHandlerConnected)
			{
				StorageService.Communication += OnStorageCommChanged;
				IsStorageCommHandlerConnected = true;
			}
			TryEnableStartButton();
		}

		public void OnServiceBound(AudioRecorderBinder binder)
		{
			AudioRecorderService = binder.Service;

			if (AudioRecorderService == null)
			{
				Log.Error(Tag, "AudioRecorderService was null!");
				return;
			}

			if (!IsAudioRecorderCommHandlerConnected)
			{
				AudioRecorderService.Communication += OnAudioRecorderCommChanged;
				IsAudioRecorderCommHandlerConnected = true;
			}
			TryEnableStartButton();
		}

		private void StopServices()
		{
			if (ServerCommService != null && ServerCommService.Status != ServerStatus.Stopped) ServerCommService.StopPost();
			if (StorageService != null && StorageService.Status != StorageStatus.Stopped) StorageService.Stop();
			if (AudioRecorderService != null && AudioRecorderService.Status != AudioRecorderStatus.Stopped) AudioRecorderService.StopRecorder();
			IsAnalyzing = false;
		}

		private void UnbindeServices()
		{
			DisconnectServiceCommHandler();

			if (ServerCommBound)
			{
				UnbindService(ServerConnection);
				ServerCommBound = false;
			}
			if (StorageBound)
			{
				UnbindService(StorageConnection);
				StorageBound = false;
			}
			if (RecorderBound)
			{
				UnbindService(AudioRecorderConnection);
				RecorderBound = false;
			}
		}

		private void DisconnectServiceCommHandler()
		{
			if (ServerCommService != null)
			{
				ServerCommService.Communication -= OnServerCommChanged;
				IsServerCommHandlerConnected = false;
			}
			if (StorageService != null)
			{
				StorageService.Communication -= OnStorageCommChanged;
				IsStorageCommHandlerConnected = false;
			}
			if (AudioRecorderService != null)
			{
				AudioRecorderService.Communication -= OnAudioRecorderCommChanged;
				IsAudioRecorderCommHandlerConnected = false;
			}
		}

		#endregion

		#region Service communication handler

		#region ServcerComm communication handler

		private void HandleError(ErrorType error)
		{
			ChangeOverlayStatusText(ErrorPopUpFactory.ResolveErrorTitle(error));
			ShowErrorViews(this, error);

			CurrentData = null;
			IsAnalyzing = false;
			ToggleKeepScreenOn(false);
		}

		private void OnServerCommChanged(object s, ServerCommEventArgs e)
		{
			switch (e.Status)
			{
				case ServerStatus.NetworkConnected:
					HandleNetworkConnectedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.NetworkConnected));
					break;
				case ServerStatus.NoNetworkConnection:
					HandleNoNetworkConnectedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.NoNetworkConnection));
					break;
				case ServerStatus.Starting:
					HandleServerCommServiceStartingStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.StartingServerCommService));
					break;
				case ServerStatus.NoServerConnection:
					HandleNoServcerConnectionStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.NoServerConnection));
					break;
				case ServerStatus.WaitingForResponse:
					HandleWaitingForResponseStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.WaitingForResponse));
					break;
				case ServerStatus.SendingSuccessful:
					HandleSendingSuccessfulState(e);
					Log.Debug(Tag, Resources.GetString(Resource.String.SendingSuccessful));
					break;
				case ServerStatus.SendingFailed:
					HandleSendingFailedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.SendingFailed));
					break;
				case ServerStatus.Stopped:
					HandleServerCommServiceStoppedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.ServerCommServiceStopped));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleServerCommServiceStartingStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.StartingServerCommService));
			ShowOverlay();
		}

		private void HandleNetworkConnectedStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.NetworkConnected));
			ShowOverlay();
		}

		private void HandleNoNetworkConnectedStatus() //TODO maybe better to try sending again instead of repeating full analysing.
		{
			HandleError(ErrorType.NoNetworkConnection);
		}

		private void HandleWaitingForResponseStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.WaitingForResponse));
			ShowOverlay();
		}

		private void HandleNoServcerConnectionStatus() //TODO maybe better to try sending again instead of repeating full analysing.
		{
			HandleError(ErrorType.NoServerConnection);
		}

		private void HandleSendingFailedStatus() //TODO maybe better to try sending again instead of repeating full analysing.
		{
			HandleError(ErrorType.SendingFailed);
		}

		private void HandleSendingSuccessfulState(ServerCommEventArgs e)
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.SendingSuccessful));
			HideOverlay();

			if (!IsInAnalyzer) return;
			if (!e.HasData)
			{
				HandleSendingFailedStatus();
				return;
			}
			CurrentData = e.Data;

			StorageService.StoreData(CurrentData);
			
		}

		private void HandleServerCommServiceStoppedStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.ServerCommServiceStopped));
			HideOverlay();
		}

		#endregion

		#region StorageService communication handler

		private void OnStorageCommChanged(object s, StorageEventArgs e)
		{
			switch (e.Status)
			{
				case StorageStatus.WriteFailed:
					HandleWriteFailedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.WriteFailed));
					break;
				case StorageStatus.ReadFailed:
					HandleReadFailedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.ReadFailed));
					break;
				case StorageStatus.WriteSuccessful:
					HandleWriteSuccessfulStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.WriteSuccessful));
					break;
				case StorageStatus.ReadSuccessful:
					HandleReadSuccessfulStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.ReadSuccessful));
					break;
				case StorageStatus.DataNotFound:
					HandleDataNotFoundStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.DataNotFound));
					break;
				case StorageStatus.DataFound:
					HandleDataFoundStatus(e);
					Log.Debug(Tag, Resources.GetString(Resource.String.DataFound));
					break;
				case StorageStatus.Starting:
					HandleStartingStorageServiceStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.StartingStorageService));
					break;
				case StorageStatus.NoDataStored:
					HandleNoDataStoredStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.NoDataStored));
					break;
				case StorageStatus.DeleteSuccessful:
					HandleDeleteSuccessfulStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.DeleteSuccessful));
					break;
				case StorageStatus.DeleteFailed:
					HandleDeleteFailedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.DeleteFailed));
					break;
				case StorageStatus.StoreFailed:
					HandleStoreFailedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.StoreFailed));
					break;
				case StorageStatus.Stopped:
					HandleStorageServiceStoppedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.StorageServiceStopped));
					break;
				case StorageStatus.StoreSuccessful:
					HandleStoreSuccessfulStatus(e);
					Log.Debug(Tag, Resources.GetString(Resource.String.StoreSuccessful));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleDataNotFoundStatus()
		{
			HandleError(ErrorType.DataNotFound);
		}

		private void HandleStartingStorageServiceStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.StartingStorageService));
			ShowOverlay();
		}

		private void HandleWriteFailedStatus()
		{
			HandleError(ErrorType.WriteFailed);
		}

		private void HandleReadFailedStatus()
		{
			HandleError(ErrorType.ReadFailed);
		}

		private void HandleStoreFailedStatus()
		{
			HandleError(ErrorType.StoreFailed);
		}

		private void HandleReadSuccessfulStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.ReadSuccessful));
			ShowOverlay();
		}

		private void HandleWriteSuccessfulStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.WriteSuccessful));
			ShowOverlay();
		}

		private void HandleStoreSuccessfulStatus(StorageEventArgs e)
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.StoreSuccessful));
			HideOverlay();

			if (!IsInAnalyzer) return;

			if (!e.HasData)
			{
				HandleStoreFailedStatus();
				return;
			}

			CurrentData = e.Data.First();

			lock (_fragmentLock)
			{
				if (CurrentData.Class == "FAILED")
				{
					((AnalyzerTabFragment) _currentFragment).AnalyzerFailed = true;
				}
				((AnalyzerTabFragment) _currentFragment).ShowResultViews(this, CurrentData.Class, SkillStoreApplication.IsHelpRequired);
			}

			ToggleKeepScreenOn(false);
			IsAnalyzing = false;
		}

		private void HandleStorageServiceStoppedStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.StorageServiceStopped));
			HideOverlay();

			OneShotTimer.Shot(state => { if (!IsInAnalyzer && IsHistoryReloadNeeded) { ReloadHistory(); } }, 100);
		}

		private void HandleNoDataStoredStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.NoDataStored));
			HideOverlay();

			if (IsInAnalyzer) return;

			lock (_fragmentLock)
			{
				((HistoryTabFragment) _currentFragment).ShowEmptyHistory(this);
			}
		}

		private void HandleDataFoundStatus(StorageEventArgs e)
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.DataFound));
			HideOverlay();

			if (IsInAnalyzer) return;

			if (e == null || !e.HasData)
			{
				HandleReadFailedStatus();
				return;
			}

			lock (_fragmentLock)
			{
				((HistoryTabFragment) _currentFragment).SetItems(this, e.Data);
			}
		}

		private void HandleDeleteFailedStatus()
		{
			HandleError(ErrorType.DeleteFailed);

			if (IsInAnalyzer) return;

			IsHistoryReloadNeeded = true;
		}

		private void HandleDeleteSuccessfulStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.DeleteSuccessful));
			HideOverlay();

			if (IsInAnalyzer) return;

			RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.DeleteSuccessful), ToastLength.Long).Show());
			IsHistoryReloadNeeded = true;
		}

		#endregion

		#region AudioRecorder communication handler

		private void OnAudioRecorderCommChanged(object s, AudioRecorderEventArgs e)
		{
			switch (e.Status)
			{
				case AudioRecorderStatus.Starting:
					HandleStartingAudioRecorderServiceStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.StartingAudioRecorderService));
					break;
				case AudioRecorderStatus.Preparing:
					HandlePreparingStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.PreparingAudioRecorderService));
					break;
				case AudioRecorderStatus.Recording:
					HandleRecordingStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.Recording));
					break;
				case AudioRecorderStatus.FinishedPreparing:
					HandleFinishedPreparingStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.FinishedPreparingAudioRecorderService));
					break;
				case AudioRecorderStatus.StoppedRecording:
					HandleStoppedRecordingStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.StoppedRecording));
					break;
				case AudioRecorderStatus.RecordingFailed:
					HandleRecordingFailedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.RecordingFailed));
					break;
				case AudioRecorderStatus.RecordingSuccessful:
					HandleRecordingSuccessfulStatus(e);
					Log.Debug(Tag, Resources.GetString(Resource.String.RecordingSuccessful));
					break;
				case AudioRecorderStatus.UpdatePrepareCounter:
					HandleUpdatePrepareCounterStatus(e);
					Log.Debug(Tag, Resources.GetString(Resource.String.UpdatePrepareCounter));
					break;
				case AudioRecorderStatus.UpdateRecordingCounter:
					HandleUpdateRecordingCounterStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.UpdateRecordingCounter));
					break;
				case AudioRecorderStatus.Stopped:
					HandleAudioRecorderServiceStoppedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.AudioRecorderStopped));
					break;
				case AudioRecorderStatus.CheckFailed:
					HandleCheckFailedStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.CheckFailed));
					break;
				case AudioRecorderStatus.CheckSuccessful:
					HandleCheckSuccessfulStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.CheckSuccessful));
					break;
				case AudioRecorderStatus.CheckingSamples:
					HandleCheckingSamplesStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.CheckingSamples));
					break;
				case AudioRecorderStatus.RecorderInitializationError:
					HandleRecorderInitializationError();
					Log.Debug(Tag, Resources.GetString(Resource.String.RecorderInitializationError));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleStartingAudioRecorderServiceStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.StartingAudioRecorderService));
			HideOverlay();
		}

		private void HandlePreparingStatus()
		{
			HideOverlay();
			
			if (!IsInAnalyzer) return;

			lock (_fragmentLock)
			{
				((AnalyzerTabFragment) _currentFragment).ShowCountdownViews(this, SkillStoreApplication.IsHelpRequired);
				((AnalyzerTabFragment) _currentFragment).ChangeStatusText(this, Resources.GetString(Resource.String.PreparingAudioRecorderService));
			}
		}


		private void HandleUpdatePrepareCounterStatus(AudioRecorderEventArgs e)
		{
			HideOverlay();
			
			if (!IsInAnalyzer) return;

			lock (_fragmentLock)
			{
				((AnalyzerTabFragment) _currentFragment).UpdateCountdownView(this, e);
			}
		}

		private void HandleFinishedPreparingStatus()
		{
			HideOverlay();
			
			if (!IsInAnalyzer) return;

			lock (_fragmentLock)
			{
				((AnalyzerTabFragment) _currentFragment).ShowAnalyzerViews(this);
			}
		}

		private void HandleRecordingStatus()
		{
			HideOverlay();
			
			if (!IsInAnalyzer) return;
			lock (_fragmentLock)
			{
				((AnalyzerTabFragment)_currentFragment).StartAnimation(this);
				((AnalyzerTabFragment)_currentFragment).ChangeStatusText(this, Resources.GetString(Resource.String.Recording));
			}
		}

		private void HandleUpdateRecordingCounterStatus()
		{
			HideOverlay();
			
			if (!IsInAnalyzer) return;

			lock (_fragmentLock)
			{
				((AnalyzerTabFragment) _currentFragment).IncrementProgressBar();
			}
		}

		private void HandleStoppedRecordingStatus()
		{
			HideOverlay();
			
			if (!IsInAnalyzer) return;

			lock (_fragmentLock)
			{
				((AnalyzerTabFragment) _currentFragment).StopAnimation(this);
			}
		}

		private void HandleRecordingFailedStatus()
		{
			HandleError(ErrorType.RecordingFailed);
		}

		private void HandleRecordingSuccessfulStatus(AudioRecorderEventArgs e)
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.RecordingSuccessful));
			HideOverlay();

			if (!IsInAnalyzer) return;
			
			if (!e.HasOutputFile || CurrentData == null || !e.AudioFilePath.Contains(CurrentData.Id))
			{
				HandleRecordingFailedStatus();
				return;
			}

			CurrentData.AudioPath = e.AudioFilePath;
			ServerCommService.PostAnalysis(CurrentData);
		}

		private void HandleAudioRecorderServiceStoppedStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.AudioRecorderStopped));
			HideOverlay();
		}

		private void HandleCheckingSamplesStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.CheckingSamples));
			ShowOverlay();
		}

		private void HandleCheckSuccessfulStatus()
		{
			ChangeOverlayStatusText(Resources.GetString(Resource.String.CheckSuccessful));
			HideOverlay();
		}

		private void HandleCheckFailedStatus()
		{
			HandleError(ErrorType.CheckFailed);
		}

		private void HandleRecorderInitializationError()
		{
			HandleError(ErrorType.RecorderInitializationError);
		}

		#endregion

		#endregion
	}
}