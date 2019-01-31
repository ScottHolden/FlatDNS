namespace FlatDNS.Core
{
	public class RecordSet
	{
		public string ID { get; set; }
		public string ETag { get; set; }
		public string Target { get; set; }
		public RecordType RecordType {get; set;}
		public long TTL { get; set; }
		public string[] Adresses { get; set; }
	}
}