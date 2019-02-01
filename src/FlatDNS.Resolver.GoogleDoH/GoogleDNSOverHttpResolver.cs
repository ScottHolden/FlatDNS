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

		public async Task<List<TargetRecord>> ResolveNameAsync(string name, RecordType type)
		{
			Response response = await ResolveAsync(name, type);

			return ParseResponse(response, type);
		}

		private List<TargetRecord> ParseResponse(Response response, RecordType type)
		{
			List<TargetRecord> records = new List<TargetRecord>(response.Answer.Length);

			foreach (Answer answer in response.Answer)
			{
				if (answer.Type == (int)type)
				{
					records.Add(new TargetRecord(answer.Data, answer.TTL));
				}
			}

			return records;
		}

		private async Task<Response> ResolveAsync(string name, RecordType type)
		{
			string endpoint = string.Format(GoogleAPI, name, type);

			string json = await _httpClient.GetStringAsync(endpoint);

			return JsonConvert.DeserializeObject<Response>(json);
		}
	}

}
