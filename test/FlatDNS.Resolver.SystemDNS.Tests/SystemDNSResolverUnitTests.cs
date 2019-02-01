using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FlatDNS.Core;
using Moq;
using Xunit;

namespace FlatDNS.Resolver.SystemDNS.Tests
{
    public class SystemDNSResolverUnitTests
    {
		[Theory]
		[InlineData("10.0.0.1", RecordType.A)]
		[InlineData("200.128.42.6", RecordType.A)]
		[InlineData("2001:db8:85a3::8a2e:370:7334", RecordType.AAAA)]
		public async Task SingleAddressCorrectRecordType(string address, RecordType recordType)
		{
			IPHostEntry hostEntry = new IPHostEntry
			{
				AddressList = new[]
				{
					IPAddress.Parse(address)
				}
			};

			Mock<ILocalDNS> mockLocalDNS = new Mock<ILocalDNS>();

			mockLocalDNS.Setup(x => x.GetHostEntryAsync(It.IsAny<string>()))
				.ReturnsAsync(hostEntry);

			SystemDNSResolver resolver = new SystemDNSResolver(mockLocalDNS.Object);

			List<TargetRecord> result = await resolver.ResolveNameAsync(It.IsAny<string>(), recordType);

			Assert.Single(result);
			Assert.Equal(address, result[0].Address);
			Assert.Null(result[0].TTL);
		}

		[Theory]
		[InlineData("200.128.42.6", RecordType.AAAA)]
		[InlineData("2001:db8:85a3::8a2e:370:7334", RecordType.A)]
		public async Task SingleAddressIncorrectRecordType(string address, RecordType recordType)
		{
			IPHostEntry hostEntry = new IPHostEntry
			{
				AddressList = new[]
				{
					IPAddress.Parse(address)
				}
			};

			Mock<ILocalDNS> mockLocalDNS = new Mock<ILocalDNS>();

			mockLocalDNS.Setup(x => x.GetHostEntryAsync(It.IsAny<string>()))
				.ReturnsAsync(hostEntry);

			SystemDNSResolver resolver = new SystemDNSResolver(mockLocalDNS.Object);

			List<TargetRecord> result = await resolver.ResolveNameAsync(It.IsAny<string>(), recordType);

			Assert.Empty(result);
		}

		[Theory]
		[InlineData("200.128.42.6", RecordType.A)]
		[InlineData("2001:db8:85a3::8a2e:370:7334", RecordType.AAAA)]
		public async Task MixedAddressAndRecordTypes(string address, RecordType recordType)
		{
			IPHostEntry hostEntry = new IPHostEntry
			{
				AddressList = new[]
				{
					IPAddress.Parse("200.128.42.6"),
					IPAddress.Parse("2001:db8:85a3::8a2e:370:7334")
				}
			};

			Mock<ILocalDNS> mockLocalDNS = new Mock<ILocalDNS>();

			mockLocalDNS.Setup(x => x.GetHostEntryAsync(It.IsAny<string>()))
				.ReturnsAsync(hostEntry);

			SystemDNSResolver resolver = new SystemDNSResolver(mockLocalDNS.Object);

			List<TargetRecord> result = await resolver.ResolveNameAsync(It.IsAny<string>(), recordType);

			Assert.Single(result);
			Assert.Equal(address, result[0].Address);
			Assert.Null(result[0].TTL);
		}
	}
}
