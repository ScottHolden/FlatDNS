using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlatDNS.Core;
using Moq;
using Xunit;

namespace FlatDNS.Resolver.SystemDNS.Tests
{
	public class SystemDNSResolverTests
	{
		private const string IPv4Address = "200.128.42.6";
		private const string IPv6Address = "2001:db8:85a3::8a2e:370:7334";

		[Fact]
		public async Task ResolveNameAsync_SingleAddress_TTLIsNull()
		{
			// Arrange

			Mock<ILocalDNS> mockLocalDNS = BuildMockLocalDNS(IPv4Address);

			SystemDNSResolver resolver = new SystemDNSResolver(mockLocalDNS.Object);

			// Act

			List<FlatTargetRecord> result = await resolver.ResolveNameAsync(It.IsAny<string>(), FlatRecordType.A);

			// Assert

			Assert.False(result.Single().TTL.HasValue);
		}

		[Theory]
		[InlineData(IPv4Address, FlatRecordType.A)]
		[InlineData(IPv6Address, FlatRecordType.AAAA)]
		public async Task ResolveNameAsync_SingleAddressCorrectRecordType_HasAdress(string address, FlatRecordType recordType)
		{
			// Arrange

			Mock<ILocalDNS> mockLocalDNS = BuildMockLocalDNS(address);

			SystemDNSResolver resolver = new SystemDNSResolver(mockLocalDNS.Object);

			// Act

			List<FlatTargetRecord> result = await resolver.ResolveNameAsync(It.IsAny<string>(), recordType);

			// Assert

			Assert.Equal(address, result.Single().Address);
		}

		[Theory]
		[InlineData(IPv4Address, FlatRecordType.AAAA)]
		[InlineData(IPv6Address, FlatRecordType.A)]
		public async Task ResolveNameAsync_SingleAddressIncorrectRecordType_DoesNotIncludeIncorrectRecords(string address, FlatRecordType recordType)
		{
			// Arrange

			Mock<ILocalDNS> mockLocalDNS = BuildMockLocalDNS(address);

			SystemDNSResolver resolver = new SystemDNSResolver(mockLocalDNS.Object);

			// Act

			List<FlatTargetRecord> result = await resolver.ResolveNameAsync(It.IsAny<string>(), recordType);

			// Assert

			Assert.Empty(result);
		}

		[Theory]
		[InlineData(IPv4Address, FlatRecordType.A)]
		[InlineData(IPv6Address, FlatRecordType.AAAA)]
		public async Task ResolveNameAsync_MixedAddressAndRecordTypes_OnlyIncludesVaildRecords(string address, FlatRecordType recordType)
		{
			// Arrange
			Mock<ILocalDNS> mockLocalDNS = BuildMockLocalDNS(IPv4Address, IPv6Address);

			SystemDNSResolver resolver = new SystemDNSResolver(mockLocalDNS.Object);

			// Act

			List<FlatTargetRecord> result = await resolver.ResolveNameAsync(It.IsAny<string>(), recordType);

			// Assert

			Assert.Equal(address, result.Single().Address);
		}

		private static Mock<ILocalDNS> BuildMockLocalDNS(params string[] addresses)
		{
			IPHostEntry hostEntry = new IPHostEntry
			{
				AddressList = addresses.Select(IPAddress.Parse).ToArray()
			};

			Mock<ILocalDNS> mockLocalDNS = new Mock<ILocalDNS>();

			mockLocalDNS.Setup(x => x.GetHostEntryAsync(It.IsAny<string>()))
				.ReturnsAsync(hostEntry);

			return mockLocalDNS;
		}
	}
}