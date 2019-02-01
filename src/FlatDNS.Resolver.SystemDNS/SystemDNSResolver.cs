using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FlatDNS.Core;

namespace FlatDNS.Resolver
{
	public class SystemDNSResolver : IResolver
	{
		private readonly ILocalDNS _localDNS;

		public SystemDNSResolver(ILocalDNS localDNS)
		{
			_localDNS = localDNS;
		}

		public SystemDNSResolver() : this(new LocalDNS())
		{
		}

		public async Task<List<FlatTargetRecord>> ResolveNameAsync(string name, FlatRecordType type)
		{
			AddressFamily adressFamily = type.ToAddressFamily();

			IPHostEntry addresses = await _localDNS.GetHostEntryAsync(name);

			return addresses.ToTargetRecord(adressFamily);
		}
	}
}