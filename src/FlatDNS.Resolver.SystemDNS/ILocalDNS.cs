using System.Net;
using System.Threading.Tasks;

namespace FlatDNS.Resolver
{
	public interface ILocalDNS
	{
		Task<IPHostEntry> GetHostEntryAsync(string name);
	}
}
