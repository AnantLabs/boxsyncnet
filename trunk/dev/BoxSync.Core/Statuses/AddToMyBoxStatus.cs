namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'add_to_mybox' web method
	/// </summary>
	public enum AddToMyBoxStatus : byte
	{
		/// <summary>
		/// Unknown status string
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 'addtomybox_ok' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// Represents 'addtomybox_error' status string
		/// </summary>
		Failed = 2,

		/// <summary>
		/// Represents ''not_logged_id' status string
		/// </summary>
		NotLoggedIn = 3,

		/// <summary>
		/// Represents 'application_restricted' status string
		/// </summary>
		ApplicationRestricted = 4,

		/// <summary>
		/// Represents 's_link_exists' status string
		/// </summary>
		LinkExists = 5
	}
}
