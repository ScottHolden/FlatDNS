using System;
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

		public async Task<List<TargetRecord>> ResolveNameAsync(string name, RecordType type)
		{
			AddressFamily adressFamily = RecordTypeToAddressFamily(type);

			IPHostEntry addresses = await _localDNS.GetHostEntryAsync(name);

			return ParseHostEntry(addresses, adressFamily);
		}

		private static AddressFamily RecordTypeToAddressFamily(RecordType type)
		{
			if (type == RecordType.A) return AddressFamily.InterNetwork;
			if (type == RecordType.AAAA) return AddressFamily.InterNetworkV6;
			// TODO: Custom exception
			throw new Exception("Unknown RecordType " + type + " no matching AddressFamily");
		}

		private static List<TargetRecord> ParseHostEntry(IPHostEntry hostEntry, AddressFamily targetAddressFamily)
		{
			// Assume that the majority of returned adresses are of the type we want
			List<TargetRecord> records = new List<TargetRecord>(hostEntry.AddressList.Length);

			foreach (IPAddress address in hostEntry.AddressList)
			{
				if (address.AddressFamily == targetAddressFamily)
				{
					records.Add(new TargetRecord(address.ToString()));
				}
			}

			return records;
		}
	}
}
