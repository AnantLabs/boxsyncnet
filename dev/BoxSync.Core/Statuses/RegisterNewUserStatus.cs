namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'register_new_user' web method
	/// </summary>
	public enum RegisterNewUserStatus : byte
	{
		/// <summary>
		/// Unknown status string
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 'successful_register' status string
		/// </summary>
		Successful = 1,

		/// <summary>
		/// Represents 'e_register' status string
		/// </summary>
		Failed = 2,

		/// <summary>
		/// Represents 'email_invalid' status string
		/// </summary>
		EmailInvalid = 3,

		/// <summary>
		/// Represents 'email_already_registered' status string
		/// </summary>
		EmailAlreadyRegistered = 4,

		/// <summary>
		/// Represents 'application_restricted' status string
		/// </summary>
		ApplicationRestricted = 5
	}
}
