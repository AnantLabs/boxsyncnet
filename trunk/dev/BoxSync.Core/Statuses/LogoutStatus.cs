namespace BoxSync.Core.Statuses
{
	/// <summary>
	/// Specifies statuses of 'logout' web method
	/// </summary>
	public enum LogoutStatus : byte 
	{
		/// <summary>
		/// Used if status string doen't match to any of enum members
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 'logout_ok' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// The user is already no longer logged into Box for your application.
		/// Represents 'not_logged_id' status string.
		/// </summary>
		NotLoggedID = 2,

		/// <summary>
		/// Provided authentication token is invalid.
		/// Represents 'invalid_auth_token' status string.
		/// </summary>
		InvalidAuthToken = 3,

		/// <summary>
		/// An invalid API key was provided, or the API key is restricted from calling this function.
		/// Represents 'application_restricted' status string.
		/// </summary>
		ApplicationRestricted = 4
	}
}
