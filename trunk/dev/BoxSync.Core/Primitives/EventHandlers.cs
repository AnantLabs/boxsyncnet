namespace BoxSync.Core.Primitives
{

	/// <summary>
	/// Represents the callback method to invoke when authrization status is updated
	/// </summary>
	/// <param name="status">Status message</param>
	public delegate void UpdateStatus(string status);

	/// <summary>
	/// Represents the callback method to invoke when authorization process is finished
	/// </summary>
	/// <param name="isSuccessful">Indicates if authorization process finished successfully</param>
	public delegate void AuthorizationProcessFinished(bool isSuccessful);

	/// <summary>
	/// Represents the callback method to invoke when operation is finished
	/// </summary>
	/// <typeparam name="TStatusType">Type of operation status</typeparam>
	/// <param name="status">Operation status</param>
	/// <param name="errorData">Error data</param>
	public delegate void OperationFinished<TStatusType>(TStatusType status, object errorData);

	/// <summary>
	/// Represents the callback method to invoke when operation is finished
	/// </summary>
	/// <typeparam name="TStatusType">Type of operation status</typeparam>
	/// <typeparam name="TResultType">Type of object which is returned by the operation as a execution result</typeparam>
	/// <param name="status">Operation status</param>
	/// <param name="result">Execution result</param>
	/// <param name="errorData">Error data</param>
	public delegate void OperationFinished<TStatusType, TResultType>(TStatusType status, TResultType result, object errorData);

	/// <summary>
	/// Defines helper methods to work with delegates of UpdateStatus type
	/// </summary>
	internal static class EventHandlerExtensions
	{
		internal static void SafeInvoke(this UpdateStatus handler, string status)
		{
			if (handler != null)
			{
				handler(status);
			}
		}
	}
}
