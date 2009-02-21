namespace BoxSync.Core.Statuses
{

	/// <summary>
	/// Specifies statuses of 'verify_registration_email' web method
	/// </summary>
	public enum VerifyRegistrationEmailStatus : byte
	{
		/// <summary>
		/// Used if status string doen't match to any of enum members
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents 'email_ok' status string
		/// </summary>
		EmailOK = 1,

		/// <summary>
		/// Represents 'email_invalid' status string
		/// </summary>
		EmailInvalid = 2,

		/// <summary>
		/// Represents 'email_already_registered' status string
		/// </summary>
		EmailAlreadyRegistered = 3,

		/// <summary>
		/// Represents 'application_restricted' status string
		/// </summary>
		ApplicationRestricted = 4
	}
}
