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
		/// Represents 'invalid_auth_token' status string
		/// </summary>
		InvalidAuthToken = 2
	}
}
