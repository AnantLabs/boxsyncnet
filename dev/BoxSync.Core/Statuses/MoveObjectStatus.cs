namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'move' web method
	/// </summary>
	public enum MoveObjectStatus : byte
	{
		/// <summary>
		/// Used if status string doen't match to any of enum members
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 's_move_node' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// Represents 'e_move_node' status string
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
