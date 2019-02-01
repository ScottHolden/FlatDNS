using System;
using System.Net.Sockets;
using FlatDNS.Core;

namespace FlatDNS.Resolver
{
	internal static class RecordTypeExtensions
	{
		public static AddressFamily ToAddressFamily(this RecordType type)
		{
			if (type == RecordType.A) return AddressFamily.InterNetwork;
			if (type == RecordType.AAAA) return AddressFamily.InterNetworkV6;

			// TODO: Custom exception, as we have matched all known types.
			throw new Exception("Unknown RecordType " + type + " no matching AddressFamily");
		}
	}
}
