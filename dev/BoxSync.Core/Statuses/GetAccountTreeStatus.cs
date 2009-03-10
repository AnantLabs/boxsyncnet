namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'get_account_tree' web method
	/// </summary>
	public enum GetAccountTreeStatus : byte
	{
		/// <summary>
		/// Unknown status string
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 'listing_ok' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// The user did not successfully authenticate.
		/// Represents 'not_logged_id' status string.
		/// </summary>
		NotLoggedID = 2,

		/// <summary>
		/// Represents 'e_folder_id' status string
		/// </summary>
		FolderIDError = 3,

		/// <summary>
		/// An invalid API key was provided, or the API key is restricted from calling this function.
		/// Represents 'application_restricted' status string.
		/// </summary>
		ApplicationRestricted = 4
	}
}