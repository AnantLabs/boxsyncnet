namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'set_description' web method
	/// </summary>
	public enum SetDescriptionStatus : byte
	{
		/// <summary>
		/// Used if status string doen't match to any of enum members
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 's_set_description' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// Represents 'e_set_description' status string
		/// </summary>
		Failed = 2
	}
}
