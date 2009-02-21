using BoxSync.Core.ServiceReference;


namespace BoxSync.Core.Primitives
{
	/// <summary>
	/// Represents the response which returns 'register_new_user' web method
	/// </summary>
	public sealed class RegisterNewUserResponse
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
