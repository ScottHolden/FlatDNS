namespace FlatDNS.Core
{
    public class TargetRecord
    {
		public long? TTL { get; }
		public string Address { get; }

		public TargetRecord(string address, long? ttl)
		{
			TTL = ttl;
			Address = address;
		}

		public TargetRecord(string address)
		{
			Address = address;
		}
    }
}
