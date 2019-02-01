using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Rest;

namespace FlatDNS.Host.Functions
{
	internal class MSITokenProvider : ITokenProvider
	{
		private const string BearerAuthenticationScheme = "Bearer";

		private readonly AzureServiceTokenProvider _tokenProvider;
		private readonly string _resource;

		public MSITokenProvider(string resouce)
		{
			_tokenProvider = new AzureServiceTokenProvider();
			_resource = resouce;
		}
		public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken)
		{
			string token = await _tokenProvider.GetAccessTokenAsync(_resource);

			return new AuthenticationHeaderValue(BearerAuthenticationScheme, token);
		}

		public static MSITokenProvider ForARM() => new MSITokenProvider("https://management.core.windows.net/");
	}
}
