using System;
using System.Net.Sockets;
using FlatDNS.Core;

namespace FlatDNS.Resolver
{
	internal static class RecordTypeExtensions
	{
		public static AddressFamily ToAddressFamily(this FlatRecordType recordType)
		{
			if (recordType == FlatRecordType.A) return AddressFamily.InterNetwork;
			if (recordType == FlatRecordType.AAAA) return AddressFamily.InterNetworkV6;

			// TODO: Custom exception, as we have matched all known types.
			throw new Exception("Unknown " + nameof(FlatRecordType) + " " + recordType + " no matching AddressFamily");
		}
	}
}
