using BoxSync.Core.Statuses;


namespace BoxSync.Core.Primitives
{
	public class GetTicketResponse : ResponseBase<GetTicketStatus>
	{
		public string Ticket
		{
			get; 
			internal set;
		}
	}
}
