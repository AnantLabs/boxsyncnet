namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'delete' web method
	/// </summary>
	public enum DeleteObjectStatus : byte
	{
		/// <summary>
		/// Unknown status string
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 's_delete_node' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// Represents 'e_delete_node' status string
		/// </summary>
		Failed = 2,

		/// <summary>
		/// Represents 'not_logged_in' status string
		/// </summary>
		NotLoggedIn = 3,

		/// <summary>
		/// Represents 'application_restricted' status string
		/// </summary>
		ApplicationRestricted = 4
	}
}
