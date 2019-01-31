using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FlatDNS.Core;

namespace FlatDNS.Resolver
{
	public class SystemDNSResolver : IResolver
	{
		public async Task<TargetRecord[]> ResolveNameAsync(string name, RecordType type)
		{
			AddressFamily typeFamily = type == RecordType.A ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;

			IPHostEntry adresses = await Dns.GetHostEntryAsync(name);

			return adresses.AddressList.Where(x => x.AddressFamily == typeFamily)
										.Select(x=> new TargetRecord
										{
											Address = x.ToString()
										})
										.ToArray();
		}
	}
}
