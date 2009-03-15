using BoxSync.Core.Statuses;


namespace BoxSync.Core.Primitives
{
	/// <summary>
	/// Represents response from 'PrivateShare' method
	/// </summary>
	public sealed class PrivateShareResponse
	{
		/// <summary>
		/// Final operation status
		/// </summary>
		public PrivateShareStatus Status
		{
			get; 
			internal set;
		}

		/// <summary>
		/// User object
		/// </summary>
		public object UserState
		{
			get;
			internal set;
		}
	}
}
