using System.Threading.Tasks;

namespace FlatDNS.Core
{
    public interface IResolver
    {
		Task<TargetRecord[]> ResolveNameAsync(string name, RecordType type);
	}
}
