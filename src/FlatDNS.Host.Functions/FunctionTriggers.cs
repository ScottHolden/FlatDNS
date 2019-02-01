using System;
using System.Threading.Tasks;
using FlatDNS.Core;
using FlatDNS.Resolver;
using FlatDNS.Zone;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace FlatDNS.Host.Functions
{
	public static class FunctionTriggers
	{
		private const string ResolverNameSetting = "ResovlerName";
		private const string TargetSubscriptionIDSetting = "TargetSubscriptionID";
		private static readonly Lazy<DNSFlattener> _flat = new Lazy<DNSFlattener>(BuildDNSFlattener);

		[FunctionName(nameof(TimerTrigger))]
		public static Task TimerTrigger(
			[TimerTrigger("0 */5 * * * *")]
				TimerInfo myTimer,
			ILogger log)
		{
			log.LogInformation($"Timer trigger executed at: {DateTime.Now}");

			return _flat.Value.ExecuteAsync();
		}

		[FunctionName(nameof(ManualHttpTrigger))]
		public static Task ManualHttpTrigger(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manual")]
				HttpRequest req,
			ILogger log)
		{
			log.LogInformation($"Manual HTTP trigger executed at: {DateTime.Now}");

			return _flat.Value.ExecuteAsync();
		}

		private static DNSFlattener BuildDNSFlattener()
		{
			string subscriptionId = GetAppSetting(TargetSubscriptionIDSetting);

			IZone zone = new AzureDNSZone(new TokenCredentials(MSITokenProvider.ForARM()), subscriptionId);
			IResolver resolver = GetResolverFromConfig();

			return new DNSFlattener(zone, resolver);
		}

		private static IResolver GetResolverFromConfig()
		{
			string resolverName = GetAppSetting(ResolverNameSetting);

			if (resolverName.Equals("SystemDNS", StringComparison.OrdinalIgnoreCase))
				return new SystemDNSResolver(new LocalDNS());

			if (resolverName.Equals("GoogleDNSOverHttp", StringComparison.OrdinalIgnoreCase))
				return new GoogleDNSOverHttpResolver();

			return new GoogleDNSOverHttpResolver();
		}

		private static string GetAppSetting(string name) =>
			Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
	}
}