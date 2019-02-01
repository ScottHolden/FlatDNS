using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using FlatDNS.Core;

namespace FlatDNS.Resolver
{
	internal static class IPHostEntryExtensions
	{
		public static List<TargetRecord> ToTargetRecord(this IPHostEntry hostEntry, AddressFamily targetAddressFamily)
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
