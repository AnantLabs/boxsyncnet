using System.Threading;

using BoxSync.Core.Primitives;
using BoxSync.Core.Statuses;

using NUnit.Framework;


namespace BoxSync.Core.IntegrationTests
{
	/// <summary>
	/// Set of tests for "GetAccountInfo" method
	/// </summary>
	public class GetAccountInfoTests : IntegrationTestBase
	{
		/// <summary>
		/// Tests synchronous "GetAccountInfo" method
		/// </summary>
		[Test]
		public void TestSyncGetAccountInfo()
		{
			BoxManager manager = new BoxManager(ApplicationKey, ServiceUrl, null);
			string ticket;
			string token;
			User user;

			manager.GetTicket(out ticket);

			SubmitAuthenticationInformation(ticket);

			manager.GetAuthenticationToken(ticket, out token, out user);

			GetAccountInfoResponse response = manager.GetAccountInfo();

			Assert.AreEqual(GetAccountInfoStatus.Successful, response.Status);

			Assert.IsNull(response.Error);
			Assert.IsNull(response.UserState);

			Assert.IsNotNull(response.User);
			
			Assert.AreEqual(user.AccessID, response.User.AccessID);
			Assert.AreEqual(user.ID, response.User.ID);
			Assert.AreEqual(user.MaxUploadSize, response.User.MaxUploadSize);
			Assert.AreEqual(user.SpaceAmount, response.User.SpaceAmount);
			Assert.AreEqual(user.SpaceUsed, response.User.SpaceUsed);

			StringAssert.IsMatch(user.Email, response.User.Email);
			StringAssert.IsMatch(user.Login, response.User.Login);
		}

		/// <summary>
		/// Tests the behavior of synchronous "GetAccountInfo" method in case if user didn't log in
		/// </summary>
		[Test]
		public void TestSyncGetAccountInfoIfUserIsNotLoggedIn()
		{
			BoxManager manager = new BoxManager(ApplicationKey, ServiceUrl, null);
			string ticket;
			
			manager.GetTicket(out ticket);

			GetAccountInfoResponse response = manager.GetAccountInfo();

			Assert.AreEqual(GetAccountInfoStatus.NotLoggedIn, response.Status);

			Assert.IsNull(response.Error);
			Assert.IsNull(response.UserState);
			Assert.IsNull(response.User);
		}

		/// <summary>
		/// Tests asynchronous "GetAccountInfo" method
		/// </summary>
		[Test]
		public void TestAsyncGetAccountInfo()
		{
			BoxManager manager = new BoxManager(ApplicationKey, ServiceUrl, null);
			string ticket;
			string token;
			User user;
			const int status = 847587532;
			ManualResetEvent wait = new ManualResetEvent(false);
			bool callbackWasExecuted = false;

			manager.GetTicket(out ticket);

			SubmitAuthenticationInformation(ticket);

			manager.GetAuthenticationToken(ticket, out token, out user);

			OperationFinished<GetAccountInfoResponse> callback = resp =>
			                                                     	{
			                                                     		Assert.AreEqual(GetAccountInfoStatus.Successful, resp.Status);
			                                                     		Assert.IsNull(resp.Error);
			                                                     		Assert.IsInstanceOfType(typeof(int), resp.UserState);
																		Assert.AreEqual(status, (int)resp.UserState);


																		Assert.IsNotNull(resp.User);



																		Assert.AreEqual(user.AccessID, resp.User.AccessID);
																		Assert.AreEqual(user.ID, resp.User.ID);
																		//Assert.AreEqual(user.MaxUploadSize, resp.User.MaxUploadSize);
																		Assert.AreEqual(user.SpaceAmount, resp.User.SpaceAmount);
																		Assert.AreEqual(user.SpaceUsed, resp.User.SpaceUsed);

																		StringAssert.IsMatch(user.Email, resp.User.Email);
																		StringAssert.IsMatch(user.Login, resp.User.Login);

			                                                     		callbackWasExecuted = true;

			                                                     		wait.Reset();
			                                                     	};

			manager.GetAccountInfo(callback, status);

			wait.WaitOne(30000);

			Assert.IsTrue(callbackWasExecuted, "Callback was not executed. The operation has timed out");
		}


		/// <summary>
		/// Tests the behavior of asynchronous "GetAccountInfo" method in case if user didn't log in
		/// </summary>
		[Test]
		public void TestAsyncGetAccountInfoIfUserIsNotLoggedIn()
		{
			BoxManager manager = new BoxManager(ApplicationKey, ServiceUrl, null);
			string ticket;
			const int status = 847587532;
			ManualResetEvent wait = new ManualResetEvent(false);
			bool callbackWasExecuted = false;

			manager.GetTicket(out ticket);

			OperationFinished<GetAccountInfoResponse> callback = resp =>
			                                                     	{
																		Assert.AreEqual(GetAccountInfoStatus.NotLoggedIn, resp.Status);

																		Assert.IsInstanceOfType(typeof(int), resp.UserState);
																		Assert.AreEqual(status, (int)resp.UserState);
																		
																		Assert.IsNull(resp.Error);
																		Assert.IsNull(resp.User);

			                                                     		callbackWasExecuted = true;

			                                                     		wait.Reset();
			                                                     	};

			manager.GetAccountInfo(callback, status);

			wait.WaitOne(30000);

			Assert.IsTrue(callbackWasExecuted, "Callback was not executed. The operation has timed out");
		}
	}
}
