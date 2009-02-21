namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'create_folder' web method
	/// </summary>
	public enum CreateFolderStatus : byte
	{
		/// <summary>
		/// Unknown status string
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 'create_ok' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// Represents 'e_no_parent_folder' status string
		/// </summary>
		NoParentFolder = 2,

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
