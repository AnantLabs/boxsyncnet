namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'copy' web method
	/// </summary>
	public enum CopyObjectStatus : byte
	{
		/// <summary>
		/// Unknown status string
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 's_copy_node' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// Represents 'e_copy_node' status string
		/// </summary>
		Failed = 2
	}
}
