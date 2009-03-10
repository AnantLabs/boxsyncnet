namespace BoxSync.Core.Statuses
{
	internal enum AuthenticationStatus : byte
	{
		Unknown = 0,
		ReadyToStartAuthentication = 1,
		GetTicketFinishedSuccessful = 2,
		GetTicketFinishedFailed = 3,
		SubmitUserCredentialsFinishedSuccessful = 4,
		SubmitUserCredentialsFailed = 5,
		GetAuthenticationTokenFinishedSuccessful = 6,
		GetAuthenticationTokenFailed = 7,
		AuthenticationFinishedSuccessfuly = 8
	}
}
