using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FlatDNS.Core;
using Newtonsoft.Json;

namespace FlatDNS.Resolver
{
	public class GoogleDNSOverHttpResolver : IResolver
	{
		// Fixed to google at the moment
		private const string GoogleAPI = "https://dns.google.com/resolve?name={0}&type{1}";

		private static readonly HttpClient _httpClient = new HttpClient();

		public async Task<List<FlatTargetRecord>> ResolveNameAsync(string name, FlatRecordType type)
		{
			Response response = await ResolveAsync(name, type);

			return response.ToFlatTargetRecord(type);
		}

		private async Task<Response> ResolveAsync(string name, FlatRecordType type)
		{
			string endpoint = string.Format(GoogleAPI, name, type);

			string json = await _httpClient.GetStringAsync(endpoint);

			return JsonConvert.DeserializeObject<Response>(json);
		}
	}

}
