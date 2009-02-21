namespace BoxSync.Core.Statuses
{
	public enum UploadFileStatus : byte
	{
		Unknown = 0,
		ApplicationRestricted = 1,
		NotLoggedID = 2,
		Failed = 4,
		Successful = 5,
		Cancelled = 6
	}
}
