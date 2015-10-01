using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using SkillStore.Error;
using SkillStore.Main;
using SkillStore.Service;
using SkillStore.Service.Communication;
using SkillStore.Service.Recorder;
using SkillStore.Service.Storage;
using SkillStore.Utility;
using Uri = Android.Net.Uri;

namespace SkillStore.Feedback
{
	[Activity(Label = "@string/FeedbackTitle", MainLauncher = false, Theme = "@style/SkillStore.Theme.Main.TabbedActivity", 
			  ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask, 
			  ParentActivity = typeof(TabbedActivity))]
	public class FeedbackActivity : Activity, ISkillStoreServiceConnectable, IErrorPopUpDismissListener
	{
		#region Attributes

		private const string Tag = "FeedbackActivity";

		private EditText _classText;
		private EditText _tagsText;
		private EditText _notesText;
		private ImageView _picturePreview;
		private ImageButton _classHelp;
		private ImageButton _tagsHelp;
		private ImageButton _pictureHelp;
		private ImageButton _notesHelp;
		private Button _cancelButton;
		private Button _sendButton;
		private TextView _statusText;
		private HelpPopUp _helpPopUp;
		private RelativeLayout _overlay;

		#endregion

		#region Properties

		private DataObject CurrentData { get; set; }
		private bool IsUiInitialized { get; set; }
		private bool IsPreviousDataLoaded { get; set; }

		public bool ServerCommBound { get; set; }
		public bool StorageBound { get; set; }
		public bool RecorderBound { get; set; }
		public bool AllServicesBound { get { return ServerCommBound && StorageBound; } }
		public ServerConnection ServerConnection { get; set; }
		public ServerCommService ServerCommService { get; set; }
		public new StorageService StorageService { get; set; }
		public StorageConnection StorageConnection { get; set; }
		public AudioRecorderConnection AudioRecorderConnection { get; set; }
		public AudioRecorderService AudioRecorderService { get; set; }

		#endregion

		#region life cycle methods

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.FeedbackActivityLayout);

			if (ActionBar != null)
			{
				ActionBar.Title = Resources.GetString(Resource.String.FeedbackTitle);
				ActionBar.SetDisplayHomeAsUpEnabled(true);
			}

			BindServices();
			ExtractExtraContent();
			InitializeUi();
		}

		protected override void OnStart()
		{
			base.OnStart();

			if (!AllServicesBound)
			{
				BindServices();
			}
		}

		protected override void OnResume()
		{
			base.OnResume();

			ConnectCommunicationHandler();
			ErrorPopUpFactory.DismissHandler += OnErrorPopUpDismissEvent;
			ConnectUi();
		}
		protected override void OnStop()
		{
			UnbindeServices();
			ErrorPopUpFactory.DismissHandler -= OnErrorPopUpDismissEvent;
			DisconnectUi();
			base.OnStop();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (requestCode != (int) ActivityRequestCode.PickImageActionFinished) return;
			if (resultCode != Result.Ok) return;
			if (data == null) return;

			var resolvedPicturePath = GetPathToImage(data.Data);
			if (resolvedPicturePath == null)
			{
				Log.Error(Tag, "There was an error loading the file");
				ShowErrorViews(this, ErrorType.PictureLoadFailed);
				return;
			}

			CurrentData.PicturePath = resolvedPicturePath;
			FillFormWithCurrentData();
		}

		private string GetPathToImage(Uri uri)
		{
			// The projection contains the columns we want to return in our query.
			var projection = new[] { MediaStore.Images.Media.InterfaceConsts.Data };
			using (var cursor = ContentResolver.Query(uri, projection, null, null, null))
			{
				if (cursor == null) return null;
				var columnIndex = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
				cursor.MoveToFirst();
				return cursor.GetString(columnIndex);
			}
		}

		private void ExtractExtraContent()
		{
			if (Intent == null)
			{
				Log.Error(Tag, "Cannot initialize _data, Intent was null");
				return;
			}

			CurrentData = new DataObject(Intent.GetStringExtra("Id"));
		}

		private void LoadPreviousData()
		{
			DisableAllViews();
			StorageService.LoadData(CurrentData);
		}

		#endregion

		#region Layout

		private void InitializeUi()
		{
			if (IsUiInitialized) return;

			_statusText = FindViewById<TextView>(Resource.Id.StatusText);

			_overlay = FindViewById<RelativeLayout>(Resource.Id.Overlay);

			_classText = FindViewById<EditText>(Resource.Id.ClassTextView);
			if (CurrentData.Class != string.Empty) _classText.Text = CurrentData.Class;

			_tagsText = FindViewById<EditText>(Resource.Id.TagsTextView);
			if (CurrentData.Tags != string.Empty) _tagsText.Text = CurrentData.Tags;

			_notesText = FindViewById<EditText>(Resource.Id.NotesTextView);
			if (CurrentData.Notes != string.Empty) _notesText.Text = CurrentData.Notes;

			_picturePreview = FindViewById<ImageView>(Resource.Id.PicturePreviewView);
			if (CurrentData.PicturePath != string.Empty) _picturePreview.SetImageURI(Uri.Parse(CurrentData.PicturePath));
			_picturePreview.Enabled = false;	//TODO remove when picture access is fixed

			_classHelp = FindViewById<ImageButton>(Resource.Id.ClassHelp);

			_tagsHelp = FindViewById<ImageButton>(Resource.Id.TagsHelp);

			_pictureHelp = FindViewById<ImageButton>(Resource.Id.PictureHelp);

			_notesHelp = FindViewById<ImageButton>(Resource.Id.NotesHelp);

			_cancelButton = FindViewById<Button>(Resource.Id.CancelButton);

			_sendButton = FindViewById<Button>(Resource.Id.SendButton);
			_sendButton.Enabled = false;

			IsUiInitialized = true;
		}

		private void ConnectUi()
		{
			_classText.AfterTextChanged += OnClassTextChanged;
			_tagsText.AfterTextChanged += OnTagsTextChanged;
			_notesText.AfterTextChanged += OnNotesTextChanged;
			_picturePreview.Click += OnPicturePreviewClicked;
			_classHelp.Click += OnClassHelpClicked;
			_tagsHelp.Click += OnTagsHelpClicked;
			_pictureHelp.Click += OnPictureHelpClicked;
			_notesHelp.Click += OnNotesHelpClicked;
			_cancelButton.Click += OnCancelClicked;
			_sendButton.Click += OnSendClicked;
		}

		private void DisconnectUi()
		{
			_classText.AfterTextChanged -= OnClassTextChanged;
			_tagsText.AfterTextChanged -= OnTagsTextChanged;
			_notesText.AfterTextChanged -= OnNotesTextChanged;
			_picturePreview.Click -= OnPicturePreviewClicked;
			_classHelp.Click -= OnClassHelpClicked;
			_tagsHelp.Click -= OnTagsHelpClicked;
			_pictureHelp.Click -= OnPictureHelpClicked;
			_notesHelp.Click -= OnNotesHelpClicked;
			_cancelButton.Click -= OnCancelClicked;
			_sendButton.Click -= OnSendClicked;
		}

		private void TryEnableSendButton()
		{
			if (!AllServicesBound) return;
			if (CurrentData != null && CurrentData.Class == string.Empty) return;
			if (!IsPreviousDataLoaded) return;

			_sendButton.Enabled = true;
		}

		private void DisableAllViews()
		{
			RunOnUiThread(() =>
			{
				_classText.Enabled = false;
				_tagsText.Enabled = false;
				_picturePreview.Enabled = false;
				_sendButton.Enabled = false;
			});
		}

		private void EnableAllViews()
		{
			RunOnUiThread(() =>
			{
				_classText.Enabled = true;
				_tagsText.Enabled = true;
				//_picturePreview.Enabled = true;  TODO remove comment after study
				TryEnableSendButton();
			});
		}

		private void FillFormWithCurrentData()
		{
			if (CurrentData == null)
			{
				Log.Error(Tag, "Filling form not possible CurrentData was null");
				return;
			}
			RunOnUiThread(() =>
			{
				if (CurrentData.Class != string.Empty) _classText.Text = CurrentData.Class;
				if (CurrentData.Tags != string.Empty) _tagsText.Text = CurrentData.Tags;
				if (CurrentData.PicturePath != string.Empty)
					_picturePreview.SetImageURI(Uri.Parse(CurrentData.PicturePath));
			});
		}

		private void ChangeStatusText(string msg)
		{
			RunOnUiThread(() => { if (_statusText != null) _statusText.Text = msg; });
		}

		private void ShowOverlay()
		{
			RunOnUiThread(() =>
			{
				DisableAllViews();
				if (_overlay != null && _overlay.Visibility != ViewStates.Visible) _overlay.Visibility = ViewStates.Visible;
			});
		}

		private void HideOverlay()
		{
			RunOnUiThread(() =>
			{
				EnableAllViews();
				if (_overlay != null && _overlay.Visibility != ViewStates.Gone) _overlay.Visibility = ViewStates.Gone;
			});
		}

		private void OnClassTextChanged(object sender, AfterTextChangedEventArgs e)
		{
			CurrentData.Class = _classText.Text;
			TryEnableSendButton();
		}

		private void OnTagsTextChanged(object sender, AfterTextChangedEventArgs e)
		{
			CurrentData.Tags = _tagsText.Text;
		}

		private void OnNotesTextChanged(object sender, AfterTextChangedEventArgs e)
		{
			CurrentData.Notes = _notesText.Text;
		}

		private void OnPicturePreviewClicked(object sender, EventArgs e)
		{
			//start camera activity -> later version
			Intent = new Intent();
			Intent.SetType("image/*");
			Intent.SetAction(Intent.ActionGetContent);
			StartActivityForResult(Intent.CreateChooser(Intent, Resources.GetString(Resource.String.SelectPicture)), (int)ActivityRequestCode.PickImageActionFinished);
		}

		private void OnClassHelpClicked(object sender, EventArgs e)
		{
			StartHelpPopUp(HelpPopUpType.ClassHelp);
		}

		private void OnTagsHelpClicked(object sender, EventArgs e)
		{
			StartHelpPopUp(HelpPopUpType.TagsHelp);
		}

		private void OnPictureHelpClicked(object sender, EventArgs e)
		{
			StartHelpPopUp(HelpPopUpType.PictureHelp);
		}

		private void OnNotesHelpClicked(object sender, EventArgs e)
		{
			StartHelpPopUp(HelpPopUpType.NotesHelp);
		}

		private void OnCancelClicked(object sender, EventArgs e)
		{
			StopServices();
			Finish();
		}

		private void OnSendClicked(object sender, EventArgs e)
		{
			ServerCommService.PostAdditionalInfo(CurrentData);
		}

		private void StartHelpPopUp(HelpPopUpType type)
		{
			switch (type)
			{
				case HelpPopUpType.ClassHelp:
					InitializeHelpPopUp(_classText, Resources.GetString(Resource.String.ClassHelpText));
					break;
				case HelpPopUpType.TagsHelp:
					InitializeHelpPopUp(_tagsText, Resources.GetString(Resource.String.TagsHelpText));
					break;
				case HelpPopUpType.PictureHelp:
					InitializeHelpPopUp(_picturePreview, Resources.GetString(Resource.String.PictureHelpText));
					break;
				case HelpPopUpType.NotesHelp:
					InitializeHelpPopUp(_notesText, Resources.GetString(Resource.String.NotesHelpText));
					break;
				default:
					throw new ArgumentOutOfRangeException("type");
			}
		}

		private void InitializeHelpPopUp(View anchor, string msg)
		{
			_helpPopUp = new HelpPopUp(this, msg);
			_helpPopUp.Show(anchor);
		}

		private void ShowErrorViews(Activity activity, ErrorType error)
		{
			ShowOverlay();
			ErrorPopUpFactory.MakeErrorPopUp(activity, error);
		}

		public void OnErrorPopUpDismissEvent(object s, EventArgs e)
		{
			HideOverlay();
		}

		#endregion

		#region Services

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
		}

		private void ConnectCommunicationHandler()
		{
			if (ServerCommService != null)
				ServerCommService.Communication += OnServerCommChanged;
			if (StorageService != null)
				StorageService.Communication += OnStorageCommChanged;
		}

		public void OnServiceBound(ServerCommBinder binder)
		{
			ServerCommService = binder.Service;

			if (ServerCommService == null)
			{
				Log.Error(Tag, "ServerCommService was null!");
				return;
			}

			ServerCommService.Communication += OnServerCommChanged;
			TryEnableSendButton();
		}

		public void OnServiceBound(StorageBinder binder)
		{
			StorageService = binder.Service;

			if (StorageService == null)
			{
				Log.Error(Tag, "StorageService was null!");
				return;
			}

			StorageService.Communication += OnStorageCommChanged;
			if (!IsPreviousDataLoaded) LoadPreviousData();

			TryEnableSendButton();
		}

		public void OnServiceBound(AudioRecorderBinder binder)
		{
			throw new NotImplementedException();
		}

		private void UnbindeServices()
		{
			if (ServerCommService != null)
				ServerCommService.Communication -= OnServerCommChanged;
			if (StorageService != null)
				StorageService.Communication -= OnStorageCommChanged;
			
			if (ServerCommBound) UnbindService(ServerConnection);
			if (StorageBound) UnbindService(StorageConnection);
			
			_sendButton.Enabled = false;
		}

		private void StopServices()
		{
			ServerCommService.StopPost();
			StorageService.Stop();
		}

		#endregion

		#region Service status handler

		#region ServerComm handler

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
			ChangeStatusText(Resources.GetString(Resource.String.StartingServerCommService));
			ShowOverlay();
		}

		private void HandleNoNetworkConnectedStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.NoNetworkConnection));
			ShowErrorViews(this, ErrorType.NoNetworkConnection);
		}

		private void HandleNetworkConnectedStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.NetworkConnected));
			ShowOverlay();
		}

		private void HandleNoServcerConnectionStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.NoServerConnection));
			ShowErrorViews(this, ErrorType.NoServerConnection);
		}

		private void HandleWaitingForResponseStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.WaitingForResponse));
			ShowOverlay();
		}

		private void HandleSendingSuccessfulState(ServerCommEventArgs e)
		{
			if (e == null || !e.HasData)
			{
				HandleSendingFailedStatus();
				return;
			}
			CurrentData = e.Data;

			ChangeStatusText(Resources.GetString(Resource.String.SendingSuccessful));
			ShowOverlay();

			StorageService.StoreData(CurrentData);
		}

		private void HandleSendingFailedStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.SendingFailed));
			ShowErrorViews(this, ErrorType.SendingFailed);
		}

		private void HandleServerCommServiceStoppedStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.ServerCommServiceStopped));
			HideOverlay();
		}

		#endregion

		#region StorageComm handler

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
					HandleStorageServiceStartingStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.StartingStorageService));
					break;
				case StorageStatus.NoDataStored:
					Log.Debug(Tag, Resources.GetString(Resource.String.NoDataStored));
					break;
				case StorageStatus.DeleteSuccessful:
					Log.Debug(Tag, Resources.GetString(Resource.String.DeleteSuccessful));
					break;
				case StorageStatus.DeleteFailed:
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
					HandleStoreSuccessfulStatus();
					Log.Debug(Tag, Resources.GetString(Resource.String.StoreSuccessful));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleStorageServiceStartingStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.StartingStorageService));
			ShowOverlay();
		}

		private void HandleStoreSuccessfulStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.StoreSuccessful));
			ShowOverlay();
			SetResult(Result.Ok, null);
			HideOverlay();
			Finish();
		}

		private void HandleStorageServiceStoppedStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.StorageServiceStopped));
			HideOverlay();
		}

		private void HandleStoreFailedStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.StoreFailed));
			ShowErrorViews(this, ErrorType.StoreFailed);
		}

		private void HandleReadSuccessfulStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.ReadSuccessful));
			ShowOverlay();
		}

		private void HandleWriteSuccessfulStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.WriteSuccessful));
			ShowOverlay();
		}

		private void HandleReadFailedStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.ReadFailed));
			ShowErrorViews(this, ErrorType.ReadFailed);
		}

		private void HandleWriteFailedStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.WriteFailed));
			ShowErrorViews(this, ErrorType.WriteFailed);
		}

		private void HandleDataNotFoundStatus()
		{
			ChangeStatusText(Resources.GetString(Resource.String.DataNotFound));
			ShowErrorViews(this, ErrorType.DataNotFound);
		}

		private void HandleDataFoundStatus(StorageEventArgs e)
		{
			if (e == null || !e.HasData)
			{
				HandleDataNotFoundStatus();
				return;
			}

			CurrentData = e.Data.First();
			IsPreviousDataLoaded = true;

			ChangeStatusText(Resources.GetString(Resource.String.DataFound));
			FillFormWithCurrentData();
		}

		#endregion

		#endregion

		#endregion
	}
}