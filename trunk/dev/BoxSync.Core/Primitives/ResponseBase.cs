namespace BoxSync.Core.Primitives
{
	/// <summary>
	/// Base type for all operation response types
	/// </summary>
	/// <typeparam name="TStatusType"></typeparam>
	public abstract class ResponseBase<TStatusType>
	{
		/// <summary>
		/// Gets operation status
		/// </summary>
		public TStatusType Status
		{
			get; 
			internal set;
		}

		/// <summary>
		/// Gets user state object
		/// </summary>
		public object UserState
		{
			get; 
			internal set;
		}
	}
}
