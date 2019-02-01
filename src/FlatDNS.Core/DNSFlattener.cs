using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlatDNS.Core
{
	public class DNSFlattener
	{
		private readonly IZone _zone;
		private readonly IResolver _resolver;

		public DNSFlattener(IZone zone, IResolver resolver)
		{
			_zone = zone;
			_resolver = resolver;
		}

		public async Task ExecuteAsync()
		{
			FlatRecordSet[] recordSets = await _zone.ListRecordSetsAsync();

			if (recordSets == null || recordSets.Length < 1) return;

			await Task.WhenAll(recordSets.Select(UpdateRecordSet));
		}

		private async Task UpdateRecordSet(FlatRecordSet set)
		{
			List<FlatTargetRecord> newAdresses = await _resolver.ResolveNameAsync(set.Target, set.RecordType);

			if (newAdresses == null || newAdresses.Count < 1)
			{
				// Couldn't resolve target
				return;
			}

			if (set.Adresses.Length == newAdresses.Count &&
				set.Adresses.OrderBy(x => x).SequenceEqual(newAdresses.Select(x => x.Address).OrderBy(x => x)))
			{
				// Adresses are the same, no work to do
				return;
			}

			await _zone.UpdateRecordSetAsync(set, newAdresses);
		}
	}
}