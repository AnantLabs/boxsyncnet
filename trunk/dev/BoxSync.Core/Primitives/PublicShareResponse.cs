using BoxSync.Core.Statuses;


namespace BoxSync.Core.Primitives
{
	public class PublicShareResponse : ResponseBase<PublicShareStatus>
	{
		/// <summary>
		/// Unique identifier of a publicly shared object
		/// </summary>
		public string PublicName
		{
			get; 
			set;
		}
	}
}
