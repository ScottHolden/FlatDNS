using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using FlatDNS.Core;

namespace FlatDNS.Resolver
{
	internal static class IPHostEntryExtensions
	{
		public static List<FlatTargetRecord> ToTargetRecord(this IPHostEntry hostEntry, AddressFamily targetAddressFamily)
		{
			// Assume that the majority of returned adresses are of the type we want
			List<FlatTargetRecord> records = new List<FlatTargetRecord>(hostEntry.AddressList.Length);

			foreach (IPAddress address in hostEntry.AddressList)
			{
				if (address.AddressFamily == targetAddressFamily)
				{
					records.Add(new FlatTargetRecord(address.ToString()));
				}
			}

			return records;
		}
	}
}
