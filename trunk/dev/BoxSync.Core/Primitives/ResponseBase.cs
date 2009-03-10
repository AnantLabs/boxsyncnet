namespace BoxSync.Core.Primitives
{
	public abstract class ResponseBase<TStatusType>
	{
		public TStatusType Status
		{
			get; 
			internal set;
		}

		public object UserState
		{
			get; 
			internal set;
		}
	}
}
