using System.Diagnostics;

using BoxSync.Core.ServiceReference;


namespace BoxSync.Core.Primitives
{
	/// <summary>
	/// Defines base properties of Box.NET folder
	/// </summary>
	[DebuggerDisplay("ID = {ID}, Name = {Name}, FolderType = {FolderTypeID}, IsShared = {IsShared}")]
	public class FolderBase
	{
		public FolderBase()
		{
			
		}

		internal FolderBase(SOAPFolder folder)
		{
			ID = folder.folder_id;
			Name = folder.folder_name;
			FolderTypeID = folder.folder_type_id;
			ParentFolderID = folder.parent_folder_id;
			Password = folder.password;
			Path = folder.path;
			PublicName = folder.public_name;
			IsShared = folder.shared == 1;
			OwnerID = folder.user_id;
		}

		/// <summary>
		/// ID of the folder
		/// </summary>
		public long ID
		{
			get;
			set;
		}

		/// <summary>
		/// Name of the folder
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Folder owner ID
		/// </summary>
		public long OwnerID
		{
			get;
			set;
		}

		/// <summary>
		/// Type of the folder. Could be null
		/// </summary>
		public long? FolderTypeID
		{
			get;
			set;
		}

		/// <summary>
		/// ID of the parent folder. Could be null
		/// </summary>
		public long? ParentFolderID
		{
			get; 
			set;
		}

		/// <summary>
		/// Folder password. Could be null
		/// </summary>
		public string Password
		{
			get; 
			set;
		}

		/// <summary>
		/// Path to the folder. Could be null
		/// </summary>
		public string Path
		{
			get; 
			set;
		}

		/// <summary>
		/// Public name of the folder. Could be null
		/// </summary>
		public string PublicName
		{
			get; 
			set;
		}

		/// <summary>
		/// Indicates if folder is shared
		/// </summary>
		public bool IsShared
		{
			get; 
			set;
		}
	}
}
