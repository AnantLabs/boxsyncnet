using BoxSync.Core.Statuses;


namespace BoxSync.Core.Primitives
{

	/// <summary>
	/// Represents response from 'FileNewCopy' method
	/// </summary>
	public sealed class FileNewCopyResponse : ResponseBase<FileNewCopyStatus>
	{
		/// <summary>
		/// Gets or sets the ID of the folder to which file(s) was (were) uploaded
		/// </summary>
		public long FolderID
		{
			get;
			internal set;
		}
	}
}
