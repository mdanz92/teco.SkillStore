using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Util;
using Java.Lang;
using SkillStore.Utility;
using Byte = System.Byte;
using Exception = System.Exception;
using Uri = System.Uri;

namespace SkillStore.Service.Communication
{
	public delegate void ServerCommEventHandler(object s, ServerCommEventArgs e);

	[Service]
	[IntentFilter(new[] { "Skillstore.Service.Communication.ServerCommService" })]
	public sealed class ServerCommService : Android.App.Service
	{
		private const string Tag = "ServerCommService";

		private DataObject _resultObject;
		private ServerStatus _status = ServerStatus.Stopped;

		public event ServerCommEventHandler Communication;

		private CommThread WorkerThread { get; set; }

		private void OnCommunication(object s, ServerCommEventArgs e)
		{
			var handler = Communication;
			if (handler != null) handler(s, e);
		}

		public ServerStatus Status
		{
			get { return _status; }
			private set
			{
				_status = value;
				OnCommunication(this, new ServerCommEventArgs(_status, _resultObject));
			}
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Debug(Tag, "Service started.");
			return StartCommandResult.Sticky;
		}

		public override IBinder OnBind(Intent intent)
		{
			return new ServerCommBinder(this);
		}

		public void PostAnalysis(DataObject data)
		{
			Status = ServerStatus.Starting;

			StopPost();

			WorkerThread = new CommThread(this, data);
			WorkerThread.Name = "PostAnalysisThread";
			WorkerThread.PostType = PostType.Analysis;
			WorkerThread.Start();
		}

		public void PostAdditionalInfo(DataObject data)
		{
			Status = ServerStatus.Starting;

			StopPost();

			WorkerThread = new CommThread(this, data);
			WorkerThread.Name = "AddInfoThread";
			WorkerThread.PostType = PostType.AdditionalInformation;
			WorkerThread.Start();
		}

		public void StopPost()
		{
			if (WorkerThread != null)
			{
				WorkerThread.RequestStop();
				WorkerThread.Join();
				if (!WorkerThread.IsAlive)
					WorkerThread = null;
			}

			Log.Debug(Tag, "ServerComm Post stopped");
		}

		#region Service worker thread

		private class CommThread : Thread
		{
			private const string ThreadTag = "CommThread";

			private readonly ServerCommService _service;
			private readonly DataObject _data;
			private readonly Uri _addInfoUri;
			private readonly Uri _analysisUri;

			public PostType PostType { private get; set; }
			private bool IsWaitingForResponse { get; set; }
			private bool IsStopRequested { get; set; }

			public CommThread(ServerCommService service, DataObject data)
			{
				_service = service;
				_addInfoUri = new Uri(_service.ApplicationContext.Resources.GetString(Resource.String.AddInfoServerAddress));
				_analysisUri = new Uri(_service.ApplicationContext.Resources.GetString(Resource.String.AnalysisServerAddress));
				_data = data;
				PostType = PostType.None;
			}

			public override void Run()
			{
				base.Run();

				if (!IsStopRequested && CheckNetworkConnection()) 
					Post();

				while (IsWaitingForResponse && !IsStopRequested)
				{
					Sleep(200);
					Log.Debug(ThreadTag, "Waiting for response...");
				}

				while (!IsStopRequested) Sleep(200);
				_service.Stopped();
			}

			private bool CheckNetworkConnection()
			{
				var connManager = (ConnectivityManager) _service.GetSystemService(ConnectivityService);
				var networkInfo = connManager.ActiveNetworkInfo;
				if (networkInfo != null && networkInfo.IsConnected)
				{
					_service.NetworkConnected();
					return true;
				}

				Log.Error(ThreadTag, "No connection to the Internet.");
				_service.NoNetworkConnection();
				RequestStop();
				return false;
			}

			private void Post()
			{
				switch (PostType)
				{
					case PostType.None:
						Log.Error(ThreadTag, "Service was started with uninitialized PostType.");
						_service.SendingFailed();
						RequestStop();
						break;
					case PostType.Analysis:
						SendAnalysisAsync();
						break;
					case PostType.AdditionalInformation:
						SendAdditionalInfoAsync();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			private async void SendAnalysisAsync()
			{
				var webClient = new HttpClient();
				var form = new MultipartFormDataContent();
				form.Add(new StringContent(_data.Id), "id");
				form.Add(new StringContent(_data.DeviceInfo), "device");

				var buffer = FileToByteArray(_data.AudioPath);
				if (buffer.Length <= 0)
				{
					Log.Error(ThreadTag, "SendAnalysisAsync: buffer was empty.");
					_service.SendingFailed();
					RequestStop();
					return;
				}
				form.Add(new ByteArrayContent(buffer), "audio", _data.AudioName);

				//////////// Summative study ResultFaker ////////////////////////////
				form.Add(new StringContent(Android.Provider.Settings.Secure.GetString(_service.ContentResolver, Android.Provider.Settings.Secure.AndroidId)), "notes");
				/////////////////////////////////////////////////////////////////////

				IsWaitingForResponse = true;
				_service.WaitingForResponse();
				var response = await webClient.PostAsync(_analysisUri, form);
				
				if (!IsStopRequested) EvaluateResponse(response);
				IsWaitingForResponse = false;
				webClient.Dispose();
			}

			private async void SendAdditionalInfoAsync()
			{
				var webClient = new HttpClient();
				var form = new MultipartFormDataContent();
				form.Add(new StringContent(_data.Id), "id");
				form.Add(new StringContent(_data.Class), "class");
				form.Add(new StringContent(_data.Tags), "tags");

				//////////// Summative study ResultFaker ////////////////////////////
				//form.Add(new StringContent(EggMapper.GetRealName(_data.Class) ?? _data.Class), "class");
				//form.Add(new StringContent(EggMapper.GetRealName(_data.Tags) ?? _data.Tags), "tags"); //Summative Eval: convert summative eval name to db name. change after summative eval.
				////////////////////////////////////////////////////////////////////
				
				form.Add(new StringContent(_data.Notes), "notes");

				if (_data.PicturePath != "")
				{
					var buffer = FileToByteArray(_data.PicturePath);
					if (buffer.Length <= 0)
					{
						Log.Error(ThreadTag, "SendAdditionalInfoAsync: buffer was empty.");
						_service.SendingFailed();
						RequestStop();
						return;
					}
					form.Add(new ByteArrayContent(buffer), "image", _data.PictureName);
				}

				IsWaitingForResponse = true;
				_service.WaitingForResponse();

				try
				{
					var response = await webClient.PostAsync(_addInfoUri, form);
					if (!IsStopRequested) EvaluateResponse(response);
				}
				catch (WebException e)
				{
					Log.Error(ThreadTag, "Error while trying to reach server." + e.Message + e.StackTrace);
					_service.SendingFailed();
					RequestStop();
				}
				catch (Exception e)
				{
					Log.Error(ThreadTag, "Error while trying to reach server." + e.Message + e.StackTrace);
					_service.SendingFailed();
					RequestStop();
				}

				IsWaitingForResponse = false;
				webClient.Dispose();
			}

			private static byte[] FileToByteArray(string path)
			{
				byte[] buffer;
				using (var fileStream = File.Open(path, FileMode.Open))
				{
					buffer = new byte[fileStream.Length];
					fileStream.Read(buffer, 0, (int)fileStream.Length);
				}
				return buffer;
			}

			private void EvaluateResponse(HttpResponseMessage response)
			{
				if (response == null) throw new ArgumentNullException("response");

				if (response.IsSuccessStatusCode)
				{
					var result = response.Content.ReadAsByteArrayAsync().Result;
					Log.Debug(ThreadTag, "Result String: " + Encoding.UTF8.GetString(result));
					ProcessResponse(result);
				}
				else if (response.StatusCode == HttpStatusCode.RequestTimeout)
				{
					Log.Error(ThreadTag, response.StatusCode + " " + response.ReasonPhrase);
					_service.NoServerConnection();
					RequestStop();
				}
				else
				{
					Log.Error(ThreadTag, "Error on web access Code " + (int)response.StatusCode + " " + response.StatusCode + ": " + response.ReasonPhrase);
					_service.SendingFailed();
					RequestStop();
				}
			}

			private void ProcessResponse(Byte[] result)
			{
				var response = DeserializeResponse(result);

				if (response == null || response.Id != _data.Id)
				{
					Log.Error(ThreadTag, "Wrong id in response or deserialization failed.");
					_service.SendingFailed();
					RequestStop();
					return;
				}

				_data.MergeData(response);
				
				/////////////////// Summative study ResultFaker /////////////////////
				//_data.Class = PostType == PostType.Analysis ? ResultFaker.GetNextResult() : ResultFaker.GetCurrentResult();
				////////////////////////////////////////////////////////////////////
				if (_data.Tags == "null")
				{
					_data.Tags = "";
				}

				_service.SendingSuccessful(_data);
				RequestStop();
			}

			private static Response DeserializeResponse(byte[] result)
			{
				Response response;
				var serializer = new DataContractJsonSerializer(typeof (ResponseRoot), new[] {typeof (Response)});
				using (var memStream = new MemoryStream(result))
				{
					memStream.Seek(0, SeekOrigin.Begin);

					try
					{
						var responseRoot = (ResponseRoot) serializer.ReadObject(memStream);
						response = responseRoot.Response;
					}
					catch (SerializationException ex)
					{
						Log.Error(ThreadTag, ex.Message);
						return null;
					}
				}
				return response;
			}

			public void RequestStop()
			{
				IsStopRequested = true;
				IsWaitingForResponse = false;
			}
		}

		#endregion

		#region Status callbacks

		private void SendingFailed()
		{
			Status = ServerStatus.SendingFailed;
		}

		private void NetworkConnected()
		{
			Status = ServerStatus.NetworkConnected;
		}

		private void NoNetworkConnection()
		{
			Status = ServerStatus.NoNetworkConnection;
		}

		private void WaitingForResponse()
		{
			Status = ServerStatus.WaitingForResponse;
		}

		private void NoServerConnection()
		{
			Status = ServerStatus.NoServerConnection;
		}

		private void SendingSuccessful(DataObject data)
		{
			_resultObject = data;
			Status = ServerStatus.SendingSuccessful;
		}

		private void Stopped()
		{
			Status = ServerStatus.Stopped;
		}

		#endregion
	}
}