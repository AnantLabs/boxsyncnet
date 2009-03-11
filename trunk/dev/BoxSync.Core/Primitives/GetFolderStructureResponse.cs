using BoxSync.Core.Statuses;


namespace BoxSync.Core.Primitives
{
	public class GetFolderStructureResponse : ResponseBase<GetAccountTreeStatus>
	{
		public Folder Folder
		{
			get; 
			set;
		}
	}
}
