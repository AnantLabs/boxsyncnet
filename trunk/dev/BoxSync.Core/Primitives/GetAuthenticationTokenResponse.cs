using BoxSync.Core.Statuses;


namespace BoxSync.Core.Primitives
{
	public class GetAuthenticationTokenResponse : ResponseBase<GetAuthenticationTokenStatus>
	{
		public User AuthenticatedUser { get; set; }

		public string AuthenticationToken { get; set; }
	}
}
