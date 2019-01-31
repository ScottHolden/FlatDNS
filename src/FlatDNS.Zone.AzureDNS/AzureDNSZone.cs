using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlatDNS.Core;
using Microsoft.Azure.Management.Dns;
using Microsoft.Rest;
using Microsoft.Rest.Azure;

namespace FlatDNS.Zone
{
	public class AzureDNSZone : IZone
	{
		public const string ZoneEnableTag = "flatdns.enabled";
		public const string RecordTargetTag = "flatdns.target";

		private readonly DnsManagementClient _dnsClient;
		public AzureDNSZone(ServiceClientCredentials credentials, string subscriptionId)
		{
			_dnsClient = new DnsManagementClient(credentials)
			{
				SubscriptionId = subscriptionId
			};
		}

		public async Task<RecordSet[]> ListRecordSetsAsync()
		{
			List<Microsoft.Azure.Management.Dns.Models.Zone> enabledZones = await ListFlatDNSEnabledZones();
			List<Microsoft.Azure.Management.Dns.Models.RecordSet>[] recordsWithTarget = await Task.WhenAll(enabledZones.Select(ListRecordsWithTargetAsync));
			return recordsWithTarget.SelectMany(x => x).Select(ParseAzureRecordSet).ToArray();
		}

		private RecordSet ParseAzureRecordSet(Microsoft.Azure.Management.Dns.Models.RecordSet set)
		{
			RecordType type = ParseType(set.Type);

			return new RecordSet
			{
				ID = set.Id,
				ETag = set.Etag,
				Target = set.Metadata[RecordTargetTag].TrimEnd('.'),
				RecordType = type,
				TTL = set.TTL ?? -1,
				Adresses = ParseAdresses(set, type)
			};
		}

		private RecordType ParseType(string type)
		{
			if (type.Equals("Microsoft.Network/dnszones/A")) return RecordType.A;
			if (type.Equals("Microsoft.Network/dnszones/AAAA")) return RecordType.AAAA;
			throw null;
		}

		private string[] ParseAdresses(Microsoft.Azure.Management.Dns.Models.RecordSet set, RecordType type)
		{
			if(type == RecordType.A) return set.ARecords.Select(x => x.Ipv4Address).ToArray();
			if (type == RecordType.AAAA) return set.AaaaRecords.Select(x => x.Ipv6Address).ToArray();
			throw null;
		}

		private Task<List<Microsoft.Azure.Management.Dns.Models.Zone>> ListFlatDNSEnabledZones() =>
			ListAllPagesWithFilter(() => _dnsClient.Zones.ListAsync(), 
				next => _dnsClient.Zones.ListNextAsync(next),
				x => x.Tags.ContainsKey(ZoneEnableTag) && x.Tags[ZoneEnableTag].Equals("true", StringComparison.OrdinalIgnoreCase));

		private Task<List<Microsoft.Azure.Management.Dns.Models.RecordSet>> ListRecordsWithTargetAsync(Microsoft.Azure.Management.Dns.Models.Zone zone) =>
			ListRecordsWithTargetAsync(zone.Id.Split('/')[4], zone.Name);

		private Task<List<Microsoft.Azure.Management.Dns.Models.RecordSet>> ListRecordsWithTargetAsync(string resourceGroup, string zoneName) =>
			ListAllPagesWithFilter(() => _dnsClient.RecordSets.ListByDnsZoneAsync(resourceGroup, zoneName),
				next => _dnsClient.RecordSets.ListAllByDnsZoneNextAsync(next),
				x => x.Metadata.ContainsKey(RecordTargetTag) && !string.IsNullOrWhiteSpace(x.Metadata[RecordTargetTag]));

		private async Task<List<T>> ListAllPagesWithFilter<T>(Func<Task<IPage<T>>> listAsync, Func<string, Task<IPage<T>>> listNextAsync, Func<T, bool> filter)
		{
			List<T> final = new List<T>();
			IPage<T> thisPage = await listAsync();

			while (true)
			{
				final.AddRange(thisPage.Where(filter));

				if (string.IsNullOrEmpty(thisPage.NextPageLink))
					break;

				thisPage = await listNextAsync(thisPage.NextPageLink);
			}

			return final;
		}

		public async Task UpdateRecordSetAsync(RecordSet set, TargetRecord[] adresses)
		{
			// If i ever find someone doing this in prod code, I'll be unhappy :P
			string[] s = set.ID.Split('/');

			Microsoft.Azure.Management.Dns.Models.RecordType recordType = Microsoft.Azure.Management.Dns.Models.RecordType.A;
			Microsoft.Azure.Management.Dns.Models.RecordSet record = new Microsoft.Azure.Management.Dns.Models.RecordSet();

			if (set.RecordType == RecordType.A)
			{
				recordType = Microsoft.Azure.Management.Dns.Models.RecordType.A;
				record.ARecords = adresses.Select(x => new Microsoft.Azure.Management.Dns.Models.ARecord(x.Address)).ToList();

			}
			else if (set.RecordType == RecordType.AAAA)
			{
				recordType = Microsoft.Azure.Management.Dns.Models.RecordType.AAAA;
				record.AaaaRecords = adresses.Select(x => new Microsoft.Azure.Management.Dns.Models.AaaaRecord(x.Address)).ToList();
			}
			else return; // And just eat it.

			if (set.TTL > 0)
			{
				long newTTL = adresses.Select(x => x.TTL ?? -1).Concat(new[] { set.TTL }).Where(x => x > 0).Min();
				record.TTL = newTTL;
			}

			// ETag will throw here
			await _dnsClient.RecordSets.UpdateAsync(s[4], s[8], s[10], recordType, record, set.ETag);
		}
	}
}
