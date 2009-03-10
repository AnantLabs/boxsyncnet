using BoxSync.Core.Statuses;

namespace BoxSync.Core.Primitives
{
	public class AuthenticateUserResponse : ResponseBase<AuthorizeStatus>
	{
		/// <summary>
		/// Authenticated user information
		/// </summary>
		public User AuthenticatedUser
		{
			get;
			internal set;
		}

		public string Token
		{
			get; 
			internal set;
		}
	}
}
