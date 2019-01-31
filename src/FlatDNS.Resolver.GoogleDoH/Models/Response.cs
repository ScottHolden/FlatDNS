namespace FlatDNS.Resolver
{
	public class Response
	{
		public int Status { get; set; }
		public bool TC { get; set; }
		public bool RD { get; set; }
		public bool RA { get; set; }
		public bool AD { get; set; }
		public bool CD { get; set; }
		public Question[] Question { get; set; }
		public Answer[] Answer { get; set; }
	}

}
