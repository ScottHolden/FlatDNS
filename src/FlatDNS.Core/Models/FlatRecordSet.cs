namespace FlatDNS.Core
{
	public class FlatRecordSet
	{
		public string ID { get; set; }
		public string ETag { get; set; }
		public string Target { get; set; }
		public FlatRecordType RecordType {get; set;}
		public long TTL { get; set; }
		public string[] Adresses { get; set; }
	}
}