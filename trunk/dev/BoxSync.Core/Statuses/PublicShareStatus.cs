namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'public_share' web method
	/// </summary>
	public enum PublicShareStatus : byte
	{
		/// <summary>
		/// Unknown status string
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 'share_ok' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// Represents 'share_error' status string
		/// </summary>
		Failed = 2,

		/// <summary>
		/// Represents 'not_logged_in' status string
		/// </summary>
		NotLoggedIn = 3,

		/// <summary>
		/// Represents 'application_restricted' status string
		/// </summary>
		ApplicationRestricted = 4,

		/// <summary>
		/// Represents 'wrong_node' status string
		/// </summary>
		WrongNode = 5
	}
}
