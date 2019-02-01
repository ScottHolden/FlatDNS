using Newtonsoft.Json;

namespace FlatDNS.Resolver
{
	public class Answer
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("type")]
		public int Type { get; set; }

		public long TTL { get; set; }

		[JsonProperty("data")]
		public string Data { get; set; }
	}
}