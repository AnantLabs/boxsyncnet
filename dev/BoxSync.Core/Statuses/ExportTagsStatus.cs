namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'export_tags' web method
	/// </summary>
	public enum ExportTagsStatus : byte
	{
		/// <summary>
		/// Unknown status string
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 'export_tags_ok' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// Represents 'not_logged_id' status string
		/// </summary>
		NotLoggedID = 2,

		/// <summary>
		/// Represents 'application_restricted' status string
		/// </summary>
		ApplicationRestricted = 3
	}
}
