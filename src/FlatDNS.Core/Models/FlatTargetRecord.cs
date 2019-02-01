namespace FlatDNS.Core
{
    public class FlatTargetRecord
    {
		public long? TTL { get; }
		public string Address { get; }

		public FlatTargetRecord(string address, long? ttl)
		{
			TTL = ttl;
			Address = address;
		}

		public FlatTargetRecord(string address)
		{
			Address = address;
		}
    }
}
