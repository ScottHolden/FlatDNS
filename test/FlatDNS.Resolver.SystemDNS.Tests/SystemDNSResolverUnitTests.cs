using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlatDNS.Core;
using Moq;
using Xunit;

namespace FlatDNS.Resolver.SystemDNS.Tests
{
    public class SystemDNSResolverUnitTests
    {
		private const string IPv4Address = "200.128.42.6";
		private const string IPv6Address = "2001:db8:85a3::8a2e:370:7334";

		[Theory]
		[InlineData(IPv4Address, FlatRecordType.A)]
		[InlineData(IPv6Address, FlatRecordType.AAAA)]
		public async Task SingleAddressCorrectRecordType(string address, FlatRecordType recordType)
		{
			// Arrange

			Mock<ILocalDNS> mockLocalDNS = BuildMockLocalDNS(address);

			SystemDNSResolver resolver = new SystemDNSResolver(mockLocalDNS.Object);

			// Act

			List<FlatTargetRecord> result = await resolver.ResolveNameAsync(It.IsAny<string>(), recordType);

			// Assert

			Assert.Single(result);
			Assert.Equal(address, result[0].Address);
			Assert.Null(result[0].TTL);
		}

		[Theory]
		[InlineData(IPv4Address, FlatRecordType.AAAA)]
		[InlineData(IPv6Address, FlatRecordType.A)]
		public async Task SingleAddressIncorrectRecordType(string address, FlatRecordType recordType)
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
		public async Task MixedAddressAndRecordTypes(string address, FlatRecordType recordType)
		{
			// Arrange
			Mock<ILocalDNS> mockLocalDNS = BuildMockLocalDNS(IPv4Address, IPv6Address);

			SystemDNSResolver resolver = new SystemDNSResolver(mockLocalDNS.Object);

			// Act

			List<FlatTargetRecord> result = await resolver.ResolveNameAsync(It.IsAny<string>(), recordType);

			// Assert

			Assert.Single(result);
			Assert.Equal(address, result[0].Address);
			Assert.Null(result[0].TTL);
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
