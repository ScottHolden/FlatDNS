using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlatDNS.Core
{
    public interface IResolver
    {
		Task<List<TargetRecord>> ResolveNameAsync(string name, RecordType type);
	}
}
