using FlatDNS.Core;
using FlatDNS.Resolver;
using FlatDNS.Zone;
using Microsoft.Rest;

namespace FlatDNS.Host.Console
{
    class Program
    {
        static void Main(string[] args)
        {
			string token = "";
			string subscriptionId = "";

			IZone zone = new AzureDNSZone(new TokenCredentials(token), subscriptionId);
			IResolver resolver = new GoogleDNSOverHttpResolver();

			new DNSFlattener(zone, resolver).ExecuteAsync().GetAwaiter().GetResult();
		}
    }
}
