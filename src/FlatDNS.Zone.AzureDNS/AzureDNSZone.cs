using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlatDNS.Core;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
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

		public async Task<FlatRecordSet[]> ListRecordSetsAsync()
		{
			List<Microsoft.Azure.Management.Dns.Models.Zone> enabledZones = await ListFlatDNSEnabledZones();
			List<RecordSet>[] recordsWithTarget = await Task.WhenAll(enabledZones.Select(ListRecordsWithTargetAsync));
			return recordsWithTarget.SelectMany(x => x).Select(ParseAzureRecordSet).ToArray();
		}

		private FlatRecordSet ParseAzureRecordSet(RecordSet set)
		{
			FlatRecordType type = ParseType(set.Type);

			return new FlatRecordSet
			{
				ID = set.Id,
				ETag = set.Etag,
				Target = set.Metadata[RecordTargetTag].TrimEnd('.'),
				RecordType = type,
				TTL = set.TTL ?? -1,
				Adresses = ParseAdresses(set, type)
			};
		}

		private FlatRecordType ParseType(string type)
		{
			if (type.Equals("Microsoft.Network/dnszones/A")) return FlatRecordType.A;
			if (type.Equals("Microsoft.Network/dnszones/AAAA")) return FlatRecordType.AAAA;
			throw null;
		}

		private string[] ParseAdresses(RecordSet set, FlatRecordType type)
		{
			if(type == FlatRecordType.A) return set.ARecords.Select(x => x.Ipv4Address).ToArray();
			if (type == FlatRecordType.AAAA) return set.AaaaRecords.Select(x => x.Ipv6Address).ToArray();
			throw null;
		}

		private Task<List<Microsoft.Azure.Management.Dns.Models.Zone>> ListFlatDNSEnabledZones() =>
			ListAllPagesWithFilter(() => _dnsClient.Zones.ListAsync(), 
				next => _dnsClient.Zones.ListNextAsync(next),
				x => x.Tags.ContainsKey(ZoneEnableTag) && x.Tags[ZoneEnableTag].Equals("true", StringComparison.OrdinalIgnoreCase));

		private Task<List<RecordSet>> ListRecordsWithTargetAsync(Microsoft.Azure.Management.Dns.Models.Zone zone) =>
			ListRecordsWithTargetAsync(zone.Id.Split('/')[4], zone.Name);

		private Task<List<RecordSet>> ListRecordsWithTargetAsync(string resourceGroup, string zoneName) =>
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

		public Task UpdateRecordSetAsync(FlatRecordSet set, FlatTargetRecord[] addresses)
		{
			// If I ever find someone doing this in prod code, I'll be unhappy :P
			string[] s = set.ID.Split('/');

			RecordType recordType = set.RecordType.ToAzureDnsRecordSet();

			RecordSet record = BuildRecordSet(set, addresses);

			// ETag will throw here
			return _dnsClient.RecordSets.UpdateAsync(s[4], s[8], s[10], recordType, record, set.ETag);
		}
		private static RecordSet BuildRecordSet(FlatRecordSet set, FlatTargetRecord[] addresses)
		{
			// Need to assert address length, and record type

			RecordSet record = new RecordSet();

			if (set.RecordType == FlatRecordType.A)
			{
				record.ARecords = addresses.Select(x => new ARecord(x.Address)).ToList();

			}
			else if (set.RecordType == FlatRecordType.AAAA)
			{
				record.AaaaRecords = addresses.Select(x => new AaaaRecord(x.Address)).ToList();
			}

			if (set.TTL > 0)
			{
				long newTTL = addresses.Select(x => x.TTL ?? -1).Concat(new[] { set.TTL }).Where(x => x > 0).Min();
				record.TTL = newTTL;
			}

			return record;
		}
	}
}
