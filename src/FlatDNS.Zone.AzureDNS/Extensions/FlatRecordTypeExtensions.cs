using System;
using FlatDNS.Core;
using Microsoft.Azure.Management.Dns.Models;

namespace FlatDNS.Zone
{
	internal static class FlatRecordTypeExtensions
	{
		public static RecordType ToAzureDnsRecordSet(this FlatRecordType recordType)
		{
			if (recordType == FlatRecordType.A) return RecordType.A;
			if (recordType == FlatRecordType.AAAA) return RecordType.AAAA;

			// TODO: Custom exception, as we have matched all known types.
			throw new Exception("Unknown " + nameof(FlatRecordType) + " " + recordType + " no matching Azure DNS Record Type");
		}
	}
}
