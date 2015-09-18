using System;
using Android.App;
using Android.Content;
using SkillStore.Utility;

namespace SkillStore.Error
{
	public delegate void ErrorPopUpDismissEventHandler(object s, EventArgs e);

	public static class ErrorPopUpFactory
	{
		private static AlertDialog _errorDialog;

		public static event ErrorPopUpDismissEventHandler DismissHandler;

		private static void OnDismiss(object s, EventArgs e)
		{
			var handler = DismissHandler;
			if (handler != null) handler(s, e);
		}

		public static void MakeErrorPopUp(Activity activity, ErrorType error)
		{
			if (activity == null) throw new ArgumentNullException("activity");

			activity.RunOnUiThread(() =>
			{
				_errorDialog = new AlertDialog.Builder(activity)
					.SetPositiveButton(Resource.String.TryAgainButtonText, OnTryAgainButtonClicked)
					.SetTitle(ResolveErrorTitle(error))
					.SetMessage(ResolveErrorMessage(error))
					.Create();
				_errorDialog.Show();
			});
		}

		private static void OnTryAgainButtonClicked(object sender, DialogClickEventArgs e)
		{
			if (_errorDialog != null) _errorDialog.Dismiss();
			_errorDialog = null;
			OnDismiss(sender, new EventArgs());
		}

		public static string ResolveErrorTitle(ErrorType error)
		{
			switch (error)
			{
				case ErrorType.RecordingFailed:
					return Application.Context.Resources.GetString(Resource.String.RecordingFailedTitle);
				case ErrorType.NoNetworkConnection:
					return Application.Context.Resources.GetString(Resource.String.NoNetworkConnectionTitle);
				case ErrorType.NoServerConnection:
					return Application.Context.Resources.GetString(Resource.String.NoServerConnectionTitle);
				case ErrorType.SendingFailed:
					return Application.Context.Resources.GetString(Resource.String.SendingFailedTitle);
				case ErrorType.StoreFailed:
					return Application.Context.Resources.GetString(Resource.String.StoreFailedTitle);
				case ErrorType.WriteFailed:
					return Application.Context.Resources.GetString(Resource.String.WriteFailedTitle);
				case ErrorType.ReadFailed:
					return Application.Context.Resources.GetString(Resource.String.ReadFailedTitle);
				case ErrorType.CheckFailed:
					return Application.Context.Resources.GetString(Resource.String.CheckFailedTitle);
				case ErrorType.DeleteFailed:
					return Application.Context.Resources.GetString(Resource.String.DeleteFailedTitle);
				case ErrorType.RecorderInitializationError:
					return Application.Context.Resources.GetString(Resource.String.RecorderInitializationErrorTitle);
				default:
					throw new ArgumentOutOfRangeException("error");
			}
		}

		private static string ResolveErrorMessage(ErrorType error)
		{
			switch (error)
			{
				case ErrorType.RecordingFailed:
					return Application.Context.Resources.GetString(Resource.String.RecordingFailedTip);
				case ErrorType.NoNetworkConnection:
					return Application.Context.Resources.GetString(Resource.String.NoNetworkConnectionTip);
				case ErrorType.NoServerConnection:
					return Application.Context.Resources.GetString(Resource.String.NoServerConnectionTip);
				case ErrorType.SendingFailed:
					return Application.Context.Resources.GetString(Resource.String.SendingFailedTip);
				case ErrorType.StoreFailed:
					return Application.Context.Resources.GetString(Resource.String.StoreFailedTip);
				case ErrorType.WriteFailed:
					return Application.Context.Resources.GetString(Resource.String.WriteFailedTip);
				case ErrorType.ReadFailed:
					return Application.Context.Resources.GetString(Resource.String.ReadFailedTip);
				case ErrorType.CheckFailed:
					return Application.Context.Resources.GetString(Resource.String.CheckFailedTip);
				case ErrorType.DeleteFailed:
					return Application.Context.Resources.GetString(Resource.String.DeleteFailedTip);
				case ErrorType.RecorderInitializationError:
					return Application.Context.Resources.GetString(Resource.String.RecorderInitializationErrorTip);
				default:
					throw new ArgumentOutOfRangeException("error");
			}
		}
	}
}