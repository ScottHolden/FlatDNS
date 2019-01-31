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
			RecordSet[] recordSets = await _zone.ListRecordSetsAsync();

			await Task.WhenAll(recordSets.Select(UpdateRecordSet));
		}

		private async Task UpdateRecordSet(RecordSet set)
		{
			TargetRecord[] newAdresses = await _resolver.ResolveNameAsync(set.Target, set.RecordType);
			
			if (set.Adresses.Length != newAdresses.Length ||
				set.Adresses.OrderBy(x=>x).SequenceEqual(newAdresses.Select(x=>x.Address).OrderBy(x=>x)))
			{
				await _zone.UpdateRecordSetAsync(set, newAdresses);
			}
		}
	}
}
