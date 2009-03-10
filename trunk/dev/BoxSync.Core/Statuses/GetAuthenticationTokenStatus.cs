namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'get_auth_token' web method
	/// </summary>
	public enum GetAuthenticationTokenStatus : byte
	{
		/// <summary>
		/// Unknown status string
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 'get_auth_token_ok' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// Represents 'get_auth_token_error' status string
		/// </summary>
		Failed = 2
	}
}
