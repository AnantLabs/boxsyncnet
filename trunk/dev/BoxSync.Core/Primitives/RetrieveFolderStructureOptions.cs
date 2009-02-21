using System;
using System.Collections.Generic;


namespace BoxSync.Core.Primitives
{
	/// <summary>
	/// Specifies options for folder structure retrieval operation
	/// </summary>
	[Flags]
	public enum RetrieveFolderStructureOptions : byte
	{
		None = 0,

		/// <summary>
		/// Indicates that only folders must be included in result tree, 
		/// no files
		/// </summary>
		NoFiles = 1,

		/// <summary>
		/// Indicates that XML that contains folder structure tree 
		/// should not be zipped
		/// </summary>
		NoZip = 2,

		/// <summary>
		/// Indicates that only one level of folder structure tree 
		/// should be retrieved, so you will get only files and 
		/// folders stored in folder which FolderID you have provided
		/// </summary>
		OneLevel = 4
	}

	/// <summary>
	/// Provides helper methods for RetrieveFolderStructureOptions enumeration list
	/// </summary>
	public static class FolderStructureRetrieveModeExtensions
	{
		public static bool Contains(this RetrieveFolderStructureOptions folderStructureOptions, RetrieveFolderStructureOptions options)
		{
			return (folderStructureOptions & options) == options;
		}

		public static string[] ToStringArray(this RetrieveFolderStructureOptions folderStructureOptions)
		{
			List<string> result = new List<string>(3);
			
			if((folderStructureOptions & RetrieveFolderStructureOptions.NoFiles) == RetrieveFolderStructureOptions.NoFiles)
			{
				result.Add("nofiles");
			}

			if ((folderStructureOptions & RetrieveFolderStructureOptions.NoZip) == RetrieveFolderStructureOptions.NoZip)
			{
				result.Add("nozip");
			}

			if ((folderStructureOptions & RetrieveFolderStructureOptions.OneLevel) == RetrieveFolderStructureOptions.OneLevel)
			{
				result.Add("onelevel");
			}

			return result.ToArray();
		}
	}
}
