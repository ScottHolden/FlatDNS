using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FlatDNS.Core.Tests
{
	public class DNSFlattenerTests
	{
		private const string IPv4Address1 = "200.128.42.6";
		private const string IPv4Address2 = "13.72.3.3";
		private const string IPv4Address3 = "163.15.195.84";
		private const string IPv4Address4 = "72.91.122.191";

		private const string IPv6Address1 = "2001:db8:85a3::8a2e:370:7334";
		private const string IPv6Address2 = "2002:db9:85a4::8a2f:371:7335";
		private const string IPv6Address3 = "2003:db9:85a4::8a2f:371:7335";
		private const string IPv6Address4 = "2003:db9:85a4::8a2f:371:7335";

		private const string HostName = "demo.com";

		[Theory]
		// Same count of addresses, different IP's
		[InlineData(FlatRecordType.A, new string[] { IPv4Address1 }, new string[] { IPv4Address2 })]
		[InlineData(FlatRecordType.A, new string[] { IPv4Address1, IPv4Address2 }, new string[] { IPv4Address3, IPv4Address4 })]
		[InlineData(FlatRecordType.AAAA, new string[] { IPv6Address1 }, new string[] { IPv6Address2 })]
		[InlineData(FlatRecordType.AAAA, new string[] { IPv6Address1, IPv6Address2 }, new string[] { IPv6Address3, IPv6Address4 })]

		// Different count of addresses, different IP's
		[InlineData(FlatRecordType.A, new string[] { IPv4Address1 }, new string[] { IPv4Address2, IPv4Address3 })]
		[InlineData(FlatRecordType.A, new string[] { IPv4Address1, IPv4Address2 }, new string[] { IPv4Address3 })]
		[InlineData(FlatRecordType.A, new string[] { IPv4Address1 }, new string[] { IPv4Address2, IPv4Address3, IPv4Address4 })]
		[InlineData(FlatRecordType.A, new string[] { IPv4Address1, IPv4Address2, IPv4Address3 }, new string[] { IPv4Address4 })]
		[InlineData(FlatRecordType.AAAA, new string[] { IPv6Address1 }, new string[] { IPv6Address2, IPv6Address3 })]
		[InlineData(FlatRecordType.AAAA, new string[] { IPv6Address1, IPv6Address2 }, new string[] { IPv6Address3 })]
		[InlineData(FlatRecordType.AAAA, new string[] { IPv6Address1 }, new string[] { IPv6Address2, IPv6Address3, IPv6Address4 })]
		[InlineData(FlatRecordType.AAAA, new string[] { IPv6Address1, IPv6Address2, IPv6Address3 }, new string[] { IPv6Address4 })]

		// Same count of addresses, mix of same and different IP's
		[InlineData(FlatRecordType.A, new string[] { IPv4Address1, IPv4Address2 }, new string[] { IPv4Address2, IPv4Address3 })]
		[InlineData(FlatRecordType.A, new string[] { IPv4Address1, IPv4Address2, IPv4Address3 }, new string[] { IPv4Address2, IPv4Address3, IPv4Address4 })]
		[InlineData(FlatRecordType.AAAA, new string[] { IPv6Address1, IPv6Address2 }, new string[] { IPv6Address2, IPv6Address3 })]
		[InlineData(FlatRecordType.AAAA, new string[] { IPv6Address1, IPv6Address2, IPv6Address3 }, new string[] { IPv6Address2, IPv6Address3, IPv6Address4 })]

		// Different count of addresses, mix of same and different IP's
		[InlineData(FlatRecordType.A, new string[] { IPv4Address1, IPv4Address2 }, new string[] { IPv4Address2, IPv4Address3, IPv4Address4 })]
		[InlineData(FlatRecordType.A, new string[] { IPv4Address1, IPv4Address2, IPv4Address3 }, new string[] { IPv4Address2, IPv4Address4 })]
		[InlineData(FlatRecordType.AAAA, new string[] { IPv6Address1, IPv6Address2 }, new string[] { IPv6Address2, IPv6Address3, IPv6Address4 })]
		[InlineData(FlatRecordType.AAAA, new string[] { IPv6Address1, IPv6Address2, IPv6Address3 }, new string[] { IPv6Address2, IPv6Address4 })]

		public async Task ExecuteAsync_NewResolvedRecords_UpdateDNSZone(FlatRecordType recordType, string[] address1, string[] address2)
		{
			Mock<IZone> zone = BuildZoneResolver(new FlatRecordSet
			{
				Adresses = address1,
				Target = HostName,
				RecordType = recordType
			});

			Mock<IResolver> resolver = BuildMockResolver(HostName, recordType, address2);

			DNSFlattener flattener = new DNSFlattener(zone.Object, resolver.Object);

			await flattener.ExecuteAsync();

			zone.Verify(x => x.UpdateRecordSetAsync(It.IsAny<FlatRecordSet>(), It.IsAny<FlatTargetRecord[]>()), Times.Once());
		}

		[Theory]
		[InlineData(FlatRecordType.A, IPv4Address1)]
		[InlineData(FlatRecordType.A, IPv4Address1, IPv4Address2)]
		[InlineData(FlatRecordType.AAAA, IPv6Address1)]
		[InlineData(FlatRecordType.AAAA, IPv6Address1, IPv6Address2)]
		public async Task ExecuteAsync_SameResolvedRecords_DoNotUpdateDNSZone(FlatRecordType recordType, params string[] address)
		{
			Mock<IZone> zone = BuildZoneResolver(new FlatRecordSet
			{
				Adresses = address,
				Target = HostName,
				RecordType = recordType
			});

			Mock<IResolver> resolver = BuildMockResolver(HostName, recordType, address);

			DNSFlattener flattener = new DNSFlattener(zone.Object, resolver.Object);

			await flattener.ExecuteAsync();

			zone.Verify(x => x.UpdateRecordSetAsync(It.IsAny<FlatRecordSet>(), It.IsAny<FlatTargetRecord[]>()), Times.Never());
		}

		[Theory]
		[InlineData(FlatRecordType.A)]
		[InlineData(FlatRecordType.AAAA)]
		public async Task ExecuteAsync_NoResolvedRecords_DoNotUpdateDNSZone(FlatRecordType recordType)
		{
			Mock<IZone> zone = BuildZoneResolver(new FlatRecordSet
			{
				Adresses = new[] { IPv4Address1 },
				Target = HostName,
				RecordType = recordType
			});

			Mock<IResolver> resolver = BuildMockResolver(HostName, recordType, new string[0]);

			DNSFlattener flattener = new DNSFlattener(zone.Object, resolver.Object);

			await flattener.ExecuteAsync();

			zone.Verify(x => x.UpdateRecordSetAsync(It.IsAny<FlatRecordSet>(), It.IsAny<FlatTargetRecord[]>()), Times.Never());
		}

		private static Mock<IZone> BuildZoneResolver(params FlatRecordSet[] recordSets)
		{
			Mock<IZone> zone = new Mock<IZone>();

			zone.Setup(x => x.ListRecordSetsAsync())
				.ReturnsAsync(recordSets);

			zone.Setup(x => x.UpdateRecordSetAsync(It.IsAny<FlatRecordSet>(), It.IsAny<FlatTargetRecord[]>()))
				.Returns(Task.CompletedTask);

			return zone;
		}

		private static Mock<IResolver> BuildMockResolver(string name, FlatRecordType recordType, params string[] targetRecords) =>
			BuildMockResolver(name, recordType, targetRecords.Select(x => new FlatTargetRecord(x)).ToList());

		private static Mock<IResolver> BuildMockResolver(string name, FlatRecordType recordType, List<FlatTargetRecord> targetRecords)
		{
			Mock<IResolver> resolver = new Mock<IResolver>();

			resolver.Setup(x => x.ResolveNameAsync(name, recordType))
				.ReturnsAsync(targetRecords);

			return resolver;
		}
	}
}