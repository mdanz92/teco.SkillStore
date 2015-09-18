using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Android.Util;
using SkillStore.Main;

namespace SkillStore.Utility
{
	[DataContract]
	public class DataObject
	{
		private const string Tag = "DataObject";

		[DataMember(Name = "id", IsRequired = true)] public string Id { get; set; }
		[DataMember(Name = "class", IsRequired = true)] public string Class { get; set; }
		[DataMember(Name = "tags", IsRequired = false)] public string Tags { get; set; }
		[DataMember(Name = "picture_path", IsRequired = false)] public string PicturePath { get; set; }
		[DataMember(Name = "confidence", IsRequired = false)] public string Confidence { get; set; }
		[DataMember(Name = "audio_path", IsRequired = false)] public string AudioPath { get; set; }
		[DataMember(Name = "device", IsRequired = false)] public string DeviceInfo { get; set; }
		[DataMember(Name = "notes", IsRequired = false)] public string Notes { get; set; }

		public string Date 
		{ 
			get
			{
				var culture = SkillStoreApplication.AppContext.Resources.GetString(Resource.String.DateCulture);
				var date = DateTime.ParseExact(Id, culture, CultureInfo.CurrentCulture).ToShortDateString();
				date += " " + DateTime.ParseExact(Id, culture, CultureInfo.CurrentCulture).ToLongTimeString();
				return date;
			} 
		}
		public string AudioName { get { return AudioPath.Split('/').Last(); } }
		public string PictureName { get { return PicturePath.Split('/').Last(); } }

		public DataObject(string id, string deviceInfo)
		{
			Id = id;
			DeviceInfo = deviceInfo;
			InitializeEmpty();
		}

		public DataObject(string id)
		{
			Id = id;
			DeviceInfo = string.Empty;
			InitializeEmpty();
		}

		private void InitializeEmpty()
		{
			Class = string.Empty;
			Tags = string.Empty;
			PicturePath = string.Empty;
			Confidence = string.Empty;
			AudioPath = string.Empty;
			Notes = string.Empty;
		}

		public void MergeData(Response response)
		{
			if (Id == response.Id)
			{
				Class = response.Class;
				Tags = response.Tags;
				Confidence = response.Confidence;
				return;
			}
			
			Log.Error(Tag, "Not able to merge: id is not equal.");
		}

		public bool MergeData(DataObject data)
		{
			if (Id == data.Id)
			{
				if (!string.IsNullOrEmpty(data.Class)) Class = data.Class;
				if (!string.IsNullOrEmpty(data.Tags)) Tags = data.Tags;
				if (!string.IsNullOrEmpty(data.PicturePath)) PicturePath = data.PicturePath;
				if (!string.IsNullOrEmpty(data.AudioPath)) AudioPath = data.AudioPath;
				return true;
			}

			Log.Error(Tag, "Not able to merge: id is not equal.");
			return false;
		}
	}
}