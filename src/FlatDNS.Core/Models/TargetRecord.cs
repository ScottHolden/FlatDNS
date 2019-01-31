using System;
using System.Collections.Generic;
using System.Text;

namespace FlatDNS.Core
{
    public class TargetRecord
    {
		public long? TTL { get; set; }
		public string Address { get; set; }
    }
}
