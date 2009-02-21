namespace BoxSync.Core.Statuses
{
	internal enum AuthorizationStatus : byte
	{
		Unknown = 0,
		ReadyToStartAuthorization = 1,
		GetTicketFinishedSuccessful = 2,
		GetTicketFinishedFailed = 3,
		SubmitUserCredentialsFinishedSuccessful = 4,
		SubmitUserCredentialsFailed = 5,
		GetAuthorizationTokenFinishedSuccessful = 6,
		GetAuthorizationTokenFailed = 7,
		AuthorizationFinishedSuccessfuly = 8
	}
}
