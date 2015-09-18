
using System.Runtime.Serialization;

namespace SkillStore.Utility
{
	[DataContract]
	public class ResponseRoot
	{
		[DataMember(Name = "response", IsRequired = true)] public Response Response { get; set; }
	}

	[DataContract]
	public class Response
	{
		[DataMember(Name = "id", IsRequired = true)] public string Id { get; set; }
		[DataMember(Name = "class", IsRequired = false)] public string Class { get; set; }
		[DataMember(Name = "tags", IsRequired = false)] public string Tags { get; set; }
		[DataMember(Name = "confidence", IsRequired = false)] public string Confidence { get; set; }
	}
}