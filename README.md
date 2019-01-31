# FlatDNS
A quick PoC for CNAME flattening on Azure DNS

This is a work in progress, your mileage may vary!

Key parts:
- FlatDNS.Zone.* - *DNS Zone Services to update, this also controls what target hostnames are*
  - FlatDNS.Zone.AzureDNS - *An Azure DNS management thing. This uses ARM resource tags to control what records to update.*
    - A `flatdns.enabled` tag should be set `true` on any Azure DNS zones you want to be updated
    - A `flatdns.target` metadata tag should be set to the target hostname on any record sets you want updated
- FlatDNS.Resolver.* - *Resolver services for looking up the target DNS record*
  - FlatDNS.Resolver.SystemDNS - *Uses machine wide DNS settings to resolve, doesn't care about TTL*
  - FlatDNS.Resolver.GoogleDoH - *Uses Googles DNS over HTTP API to resolve target*
- FlatDNS.Host.* - *Ways to run the app*
  - FlatDNS.Host.Console - *Quick debug console*
  - FlatDNS.Host.Functions - *Example of running it on a schedule within Azure Functions*
    - This uses MSI for idenitity
    - The App Setting `TargetSubscriptionID` is required to point the zone management to the right subscription
    - The App Setting `ResovlerName` is optional, and can change what resolver to use. Your options are `GoogleDNSOverHttp` (default) or `SystemDNS`
    - At the moment (as there is only AzureDNS), you can not change zone management providers via config
