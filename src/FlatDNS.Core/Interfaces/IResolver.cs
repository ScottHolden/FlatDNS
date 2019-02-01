using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlatDNS.Core
{
    public interface IResolver
    {
		Task<List<FlatTargetRecord>> ResolveNameAsync(string name, FlatRecordType type);
	}
}
