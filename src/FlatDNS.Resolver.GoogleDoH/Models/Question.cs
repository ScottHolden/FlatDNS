using Newtonsoft.Json;

namespace FlatDNS.Resolver
{
	public class Question
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("type")]
		public int Type { get; set; }
	}
}