using BoxSync.Core.Statuses;


namespace BoxSync.Core.Primitives
{
	public sealed class CreateFolderResponse : ResponseBase<CreateFolderStatus>
	{

		/// <summary>
		/// Gets information about created folder
		/// </summary>
		public FolderBase Folder
		{
			get; 
			internal set;
		}
	}
}
