using System.Threading.Tasks;

namespace FlatDNS.Core
{
    public interface IZone
    {
		Task<FlatRecordSet[]> ListRecordSetsAsync();
		Task UpdateRecordSetAsync(FlatRecordSet set, FlatTargetRecord[] adresses);
	}
}
