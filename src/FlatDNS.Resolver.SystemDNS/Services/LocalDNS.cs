using System.Net;
using System.Threading.Tasks;

namespace FlatDNS.Resolver
{
	public class LocalDNS : ILocalDNS
	{
		public Task<IPHostEntry> GetHostEntryAsync(string name) => Dns.GetHostEntryAsync(name);
	}
}
