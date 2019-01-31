using System.Threading.Tasks;

namespace FlatDNS.Core
{
    public interface IZone
    {
		Task<RecordSet[]> ListRecordSetsAsync();
		Task UpdateRecordSetAsync(RecordSet set, TargetRecord[] adresses);
	}
}
