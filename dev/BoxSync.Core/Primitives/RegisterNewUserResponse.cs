using BoxSync.Core.ServiceReference;
using BoxSync.Core.Statuses;


namespace BoxSync.Core.Primitives
{
	/// <summary>
	/// Represents the response which returns 'register_new_user' web method
	/// </summary>
	public sealed class RegisterNewUserResponse : ResponseBase<RegisterNewUserStatus>
	{
		/// <summary>
		/// Gets or sets authorization token
		/// </summary>
		public string Token
		{
			get; 
			set;
		}

		/// <summary>
		/// Gets or sets user information
		/// </summary>
		public SOAPUser User
		{
			get; 
			set;
		}
	}
}
