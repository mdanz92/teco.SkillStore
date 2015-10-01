using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Java.Lang;
using SkillStore.Utility;
using Environment = Android.OS.Environment;
using Exception = System.Exception;
using File = Java.IO.File;

namespace SkillStore.Service.Storage
{
	public delegate void StorageEventHandler(object s, StorageEventArgs e);

	[Service]
	[IntentFilter(new[] { "Skillstore.Service.Storage.StorageService" })]
	public class StorageService : Android.App.Service
	{
		private const string Tag = "StorageService";

		private List<DataObject> _dataObjects;
		private StorageStatus _status = StorageStatus.Stopped;

		public event StorageEventHandler Communication;

		private StorageThread WorkerThread { get; set; }

		private void OnCommunication(object s, StorageEventArgs e)
		{
			var handler = Communication;
			if (handler != null) handler(s, e);
		}

		public StorageStatus Status
		{
			get { return _status; }
			private set
			{
				_status = value;
				OnCommunication(this, new StorageEventArgs(_status, _dataObjects));
			}
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Debug(Tag, "Service started.");
			return StartCommandResult.Sticky;
		}

		public override IBinder OnBind(Intent intent)
		{
			return new StorageBinder(this);
		}

		public void StoreData(DataObject dataObject)
		{
			Stop();

			Status = StorageStatus.Starting;

			WorkerThread = new StorageThread(this);
			WorkerThread.Name = "StoreThread";
			WorkerThread.InitializeStorageInteraction(StorageRequestType.Store, dataObject);
			WorkerThread.Start();
		}

		public void LoadData(DataObject dataObject)
		{
			Stop();

			Status = StorageStatus.Starting;

			WorkerThread = new StorageThread(this);
			WorkerThread.Name = "LoadThread";
			WorkerThread.InitializeStorageInteraction(StorageRequestType.Load, dataObject);
			WorkerThread.Start();
		}

		public void LoadAllData()
		{
			Stop();

			Status = StorageStatus.Starting;

			WorkerThread = new StorageThread(this);
			WorkerThread.Name = "LoadAllThread";
			WorkerThread.InitializeStorageInteraction(StorageRequestType.LoadAll);
			WorkerThread.Start();
		}

		public void DeleteData(DataObject dataObject)
		{
			Stop();

			Status = StorageStatus.Starting;

			WorkerThread = new StorageThread(this);
			WorkerThread.Name = "DeleteThread";
			WorkerThread.InitializeStorageInteraction(StorageRequestType.Delete, dataObject);
			WorkerThread.Start();
		}

		public void DeleteAllData()
		{
			Stop();
		
			Status = StorageStatus.Starting;
			
			WorkerThread = new StorageThread(this);
			WorkerThread.Name = "DeleteAllThread";
			WorkerThread.InitializeStorageInteraction(StorageRequestType.DeleteAll);
			WorkerThread.Start();
		}

		public void Stop()
		{
			if (WorkerThread != null)
			{
				WorkerThread.RequestStop();
				WorkerThread.Join();
				if (!WorkerThread.IsAlive)
					WorkerThread = null;
			}

			Log.Debug(Tag, "StorageService stopped");
		}

		#region Service work thread

		private sealed class StorageThread : Thread
		{
			private const string ThreadTag = "StorageThread";
			private const string AudioDirPath = "audio/";
			private const string PictureDirPath = "img/";
			private const string TmpDirPath = "tmp/";

			private readonly StorageService _service;
			private DataObject _data;
			private string _storagePath;
			private string _filePath;

			private StorageRequestType RequestType { get; set; }
			private bool IsStopRequested { get; set; }

			public StorageThread(StorageService service)
			{
				_service = service;
				RequestType = StorageRequestType.None;
				SetupStorageDir();
			}

			private void SetupStorageDir()
			{
				if (Environment.ExternalStorageDirectory.Path != null)
					_storagePath = Environment.ExternalStorageDirectory.Path + File.Separator + _service.Resources.GetString(Resource.String.PathOnDevice);
				else
					_storagePath = _service.FilesDir.Path + _service.Resources.GetString(Resource.String.PathOnDevice);

				if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
				if (!Directory.Exists(_storagePath + AudioDirPath)) Directory.CreateDirectory(_storagePath + AudioDirPath);
				if (!Directory.Exists(_storagePath + PictureDirPath)) Directory.CreateDirectory(_storagePath + PictureDirPath);
			}

			public void InitializeStorageInteraction(StorageRequestType requestType, DataObject data = null)
			{
				switch (requestType)
				{
					case StorageRequestType.Store:
						InitializeStoreRequest(data);
						break;
					case StorageRequestType.LoadAll:
						InitializeLoadAllRequest();
						break;
					case StorageRequestType.Load:
						InitializeLoadRequest(data);
						break;
					case StorageRequestType.DeleteAll:
						InitializeDeleteAllRequest();
						break;
					case StorageRequestType.Delete:
						InitializeDeleteRequest(data);
						break;
					case StorageRequestType.None:
						RequestType = StorageRequestType.None;
						Log.Debug(ThreadTag, "InitializeStorageInteraction with None called.");
						break;
					default:
						throw new ArgumentOutOfRangeException("requestType");
				}
			}

			private void InitializeStoreRequest(DataObject data)
			{
				if (data == null)
				{
					Log.Error(ThreadTag, "Initialization failed: data was null.");
					return;
				}
				_data = data;
				RequestType = StorageRequestType.Store;
			}

			private void InitializeLoadRequest(DataObject data)
			{
				if (data == null)
				{
					Log.Error(ThreadTag, "Initialization failed: data was null.");
					return;
				}
				_data = data;
				RequestType = StorageRequestType.Load;
			}

			private void InitializeLoadAllRequest()
			{
				RequestType = StorageRequestType.LoadAll;
			}

			private void InitializeDeleteRequest(DataObject data)
			{
				if (data == null)
				{
					Log.Error(ThreadTag, "Initialization failed: data was null.");
					return;
				}
				_data = data;
				RequestType = StorageRequestType.Delete;
			}

			private void InitializeDeleteAllRequest()
			{
				RequestType = StorageRequestType.DeleteAll;
			}

			public override void Run()
			{
				base.Run();

				if (!IsStopRequested)
					StartStorageRequest();

				while (!IsStopRequested) Sleep(200);
				_service.Stopped();
			}

			private void StartStorageRequest()
			{
				switch (RequestType)
				{
					case StorageRequestType.Store:
						Store();
						break;
					case StorageRequestType.LoadAll:
						LoadAll();
						break;
					case StorageRequestType.Load:
						Load();
						break;
					case StorageRequestType.DeleteAll:
						DeleteAll();
						break;
					case StorageRequestType.Delete:
						Delete();
						break;
					case StorageRequestType.None:
						Log.Error(ThreadTag, "Service started uninitialized.");
						RequestStop();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			private void Store()
			{
				if (_data == null)
				{
					Log.Error(ThreadTag, "Store called with uninitialized _data.");
					_service.StoreFailed();
					RequestStop();
					return;
				}

				WriteToStorage(_data.Id);
			}

			private void Load()
			{
				if (_data == null)
				{
					Log.Error(ThreadTag, "Load called with uninitialized _data.");
					_service.DataNotFound();
					RequestStop();
					return;
				}

				LoadFromStorage(_data.Id);
			}

			private void LoadAll()
			{
				var files = Directory.GetFiles(_storagePath);
				if (files.Length <= 0)
				{
					_service.NoDataStored();
					RequestStop();
					return;
				}

				var objects = ReadObjectsFromFiles(files);
				if (objects == null)
				{
					Log.Error(ThreadTag, "Error while reading files from directory");
					_service.ReadFailed();
					RequestStop();
					return;
				}

				_service.DataFound(objects);
				RequestStop();
			}

			private void Delete()
			{
				if (!CheckStorageForId(_data.Id))
				{
					Log.Error(ThreadTag, "The data to delete does not exist");
					_service.DataNotFound();
					RequestStop();
					return;
				}

				var deleteError = !DeleteFileFromDirectory(_storagePath + _data.Id + ".json");
				deleteError |= !DeleteFileFromDirectory(_storagePath + AudioDirPath + _data.Id + ".wav");
				deleteError |= !DeleteFileFromDirectory(_storagePath + PictureDirPath + _data.Id + ".jpg");
				if (deleteError)
				{
					_service.DeleteFailed();
					RequestStop();
					return;
				}

				if (CheckStorageForId(_data.Id))
				{
					Log.Error(ThreadTag, "The data to delete still exist after delete.");
					_service.DeleteFailed();
					RequestStop();
					return;
				}

				_service.DeleteSuccessful();
				RequestStop();
			}



			private void DeleteAll()
			{
				var deleteError = !DeleteFilesFromDirectory(_storagePath);
				deleteError |= !DeleteFilesFromDirectory(_storagePath + AudioDirPath);
				deleteError |= !DeleteFilesFromDirectory(_storagePath + PictureDirPath);
				deleteError |= !DeleteFilesFromDirectory(_storagePath + TmpDirPath);
				if (deleteError)
				{
					_service.DeleteFailed();
					RequestStop();
					return;
				}

				_service.DeleteSuccessful();
				RequestStop();
			}

			private void WriteToStorage(string id)
			{
				if (CheckStorageForId(id))
					WriteToExistingFile();
				else
					WriteToNewFile();
			}

			private void LoadFromStorage(string id)
			{
				if (!CheckStorageForId(id))
				{
					Log.Error(ThreadTag, "The data requested doesn't exist.");
					_service.DataNotFound();
					RequestStop();
					return;
				}

				var dataObject = DeserializeFile(_filePath);
				if (dataObject == null)
				{
					Log.Error("StorageThread", "dataObject was null after read from file.");
					_service.ReadFailed();
					RequestStop();
					return;
				}
				var dataList = new List<DataObject> { dataObject };
				_service.DataFound(dataList);
				RequestStop();
			}

			private bool CheckStorageForId(string id)
			{
				if (id == string.Empty)
				{
					Log.Error(ThreadTag, "invalid id.");
					return false;
				}

				var fileName = id + ".json";
				_filePath = Path.Combine(_storagePath, fileName);
				return System.IO.File.Exists(_filePath);
			}

			private void WriteToExistingFile()
			{
				var dataObject = DeserializeFile(_filePath);
				if (dataObject == null)
				{
					Log.Error("StorageThread", "dataObject was null after read from file.");
					_service.ReadFailed();
					RequestStop();
					return;
				}

				_service.ReadSuccessful();

				if (!dataObject.MergeData(_data))
				{
					Log.Error(ThreadTag, "WriteToExistingFile: merge failed data.");
					_service.WriteFailed();
					RequestStop();
					return;
				}

				StoreData(dataObject);
			}

			private void WriteToNewFile()
			{
				var dataObject = new DataObject(_data.Id, _data.DeviceInfo);
				if (!dataObject.MergeData(_data))
				{
					Log.Error(ThreadTag, "WriteToNewFile: merge failed data.");
					_service.WriteFailed();
					RequestStop();
					return;
				}

				StoreData(dataObject);
			}

			//private DataObject ReadObjectFromFile(string filePath)
			//{
			//	return DeserializeFile(filePath);
			//}

			private static List<DataObject> ReadObjectsFromFiles(IEnumerable<string> files)
			{
				if (files != null) return files.Select(DeserializeFile).ToList();
				
				Log.Error("StorageThread", "files was null");
				return null;
			}

			private static DataObject DeserializeFile(string filePath)
			{
				try
				{
					var serializer = new DataContractJsonSerializer(typeof(DataObject));
					using (var fileStream = new FileStream(filePath, FileMode.Open))
					{
						return (DataObject)serializer.ReadObject(fileStream);
					}
				}
				catch (Exception e)
				{
					Log.Error(ThreadTag, e.Message + e.StackTrace);
					return null;
				}
			}

			//private DataObject EvaluateDeserializedObject(DataObject dataObject)
			//{
			//	if (dataObject != null) return dataObject;
			//	Log.Error(ThreadTag, "Read object from file failed.");
			//	return null;
			//}

			private void StoreData(DataObject dataObject)
			{
				dataObject = CopyToStorage(dataObject);
				if (dataObject == null)
				{
					Log.Error(ThreadTag, "CopyToStorageFailed.");
					_service.StoreFailed();
					RequestStop();
					return;
				}

				var writeFlag = WriteToFile(dataObject);
				if (!writeFlag)
				{
					Log.Error(ThreadTag, "WriteToFile failed.");
					_service.StoreFailed();
					RequestStop();
					return;
				}
				_service.StoreSuccessful(dataObject);
				RequestStop();
			}

			private bool WriteToFile(DataObject dataObject)
			{
				var serializer = new DataContractJsonSerializer(typeof (DataObject));
				try
				{
					using (var memStream = new MemoryStream())
					{
						serializer.WriteObject(memStream, dataObject);
						 return WriteToFileOutputStream(memStream);
					}
				}
				catch (Exception e)
				{
					Log.Error("StorageThread", e.Message + e.StackTrace);
					_service.WriteFailed();
					RequestStop();
					return false;
				}
			}

			private DataObject CopyToStorage(DataObject dataObject)
			{
				var newAudioPath = CopyAudioToStorage(dataObject);
				if (string.IsNullOrEmpty(newAudioPath))
				{
					Log.Error(ThreadTag, "CopyAudioToStorage failed.");
					return null;
				}
				dataObject.AudioPath = newAudioPath;

				var newPicturePath = CopyPictureToStorage(dataObject);
				if (newPicturePath == null)
				{
					Log.Error(ThreadTag, "CopyPictureToStorage failed.");
					return null;
				}
				dataObject.PicturePath = newPicturePath;

				//_service.WriteSuccessful();
				return dataObject;
			}

			private bool WriteToFileOutputStream(MemoryStream memStream)
			{
				if (_filePath == null)
				{
					Log.Error(ThreadTag, "_filePath null when attempting file write.");
					_service.WriteFailed();
					RequestStop();
					return false;
				}

				using (var fileOutputStream = new FileStream(_filePath, FileMode.OpenOrCreate))
				{
					if (memStream != null) memStream.WriteTo(fileOutputStream);
				}
				_service.WriteSuccessful();
				return true;
			}

			private string CopyAudioToStorage(DataObject data)
			{
				if (string.IsNullOrEmpty(data.AudioPath))
				{
					Log.Error(ThreadTag, "cannot copy audio to storage: invalid path");
					return null;
				}

				var newPath = _storagePath + AudioDirPath + data.AudioName;
				if (!System.IO.File.Exists(data.AudioPath))
				{
					Log.Error(ThreadTag, "Audio file to move does not exist.");
					return null;
				}
				if (newPath == data.AudioPath)
				{
					return newPath;
				}

				System.IO.File.Move(data.AudioPath, newPath);
				
				if (System.IO.File.Exists(newPath))
				{
					System.IO.File.Delete(data.AudioPath);
					return newPath;
				}

				Log.Error(ThreadTag, "Move audio file failed.");
				return null;
			}

			private string CopyPictureToStorage(DataObject data)
			{
				if (data.PicturePath == null)
				{
					Log.Error(ThreadTag, "cannot copy audio to storage: invalid path");
					return null;
				}

				if (data.PicturePath == string.Empty) return "";

				var newPath = _storagePath + PictureDirPath + data.PictureName;
				if (!System.IO.File.Exists(data.PicturePath))
				{
					Log.Error(ThreadTag, "Picture file to move does not exist.");
					return null;
				}
				System.IO.File.Move(data.PicturePath, newPath);
				
				if (System.IO.File.Exists(newPath))
				{
					System.IO.File.Delete(data.PicturePath);
					return newPath;
				}

				Log.Error(ThreadTag, "Move picture file failed.");
				return null;
			}

			private bool DeleteFileFromDirectory(string filePath)
			{
				System.IO.File.Delete(filePath);
				if (!System.IO.File.Exists(filePath)) return true;

				Log.Error(ThreadTag, "The file to delete still exist after delete.");
				//_service.DeleteFailed();
				return false;
			}

			private bool DeleteFilesFromDirectory(string dirPath)
			{
				var files = Directory.GetFiles(dirPath);
				if (files.Length == 0)
				{
					Log.Error(ThreadTag, "There was no data in storage");
					_service.NoDataStored();
					return true;
				}

				if (files.Any(file => !DeleteFileFromDirectory(file))) return false;

				files = Directory.GetFiles(dirPath);
				if (files.Length == 0) return true;

				Log.Error(ThreadTag, "Threr is still data in storage");
				//_service.DeleteFailed();
				return false;
			}

			public void RequestStop()
			{
				//Interrupt();
				IsStopRequested = true;
			}
		}

		#endregion

		#region Status callbacks

		private void StoreFailed()
		{
			Status = StorageStatus.StoreFailed;
		}

		private void WriteFailed()
		{
			Status = StorageStatus.WriteFailed;
		}

		private void WriteSuccessful()
		{
			Status = StorageStatus.WriteSuccessful;
		}

		private void ReadFailed()
		{
			Status = StorageStatus.ReadFailed;
		}

		private void ReadSuccessful()
		{
			Status = StorageStatus.ReadSuccessful;
		}

		private void StoreSuccessful(DataObject data)
		{
			_dataObjects = new List<DataObject>();
			_dataObjects.Add(data);
			Status = StorageStatus.StoreSuccessful;
		}

		private void DataNotFound()
		{
			Status = StorageStatus.DataNotFound;
		}

		private void DataFound(List<DataObject> dataList)
		{
			_dataObjects = dataList;
			Status = StorageStatus.DataFound;
		}

		private void NoDataStored()
		{
			Status = StorageStatus.NoDataStored;
		}

		private void DeleteFailed()
		{
			Status = StorageStatus.DeleteFailed;
		}

		private void DeleteSuccessful()
		{
			Status = StorageStatus.DeleteSuccessful;
		}

		private void Stopped()
		{
			Status = StorageStatus.Stopped;
		}

		#endregion
	}
}