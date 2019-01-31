using System;
using System.Linq;
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

		private static readonly HttpClient httpClient = new HttpClient();
		public async Task<TargetRecord[]> ResolveNameAsync(string name, RecordType type)
		{
			string json = await httpClient.GetStringAsync(string.Format(GoogleAPI, name, type));

			Response response = JsonConvert.DeserializeObject<Response>(json);

			return response.Answer.Where(x => x.Type == (int)type).Select(x => new TargetRecord
			{
				Address = x.Data,
				TTL = x.TTL
			}).ToArray();
		}
	}

}
