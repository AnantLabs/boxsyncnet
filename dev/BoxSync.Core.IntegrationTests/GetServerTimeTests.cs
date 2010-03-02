using System;
using BoxSync.Core.Primitives;
using BoxSync.Core.Statuses;

using NUnit.Framework;


namespace BoxSync.Core.IntegrationTests
{
	/// <summary>
	/// Set of tests for "GetServerTime" web-method
	/// </summary>
	[TestFixture]
	public class GetServerTimeTests : IntegrationTestBase
	{
		/// <summary>
		/// Tests successful scenario of "GetServerTime" method execution
		/// </summary>
		[Test]
		public void TestGetServerTime()
		{
			BoxManager manager = new BoxManager(ApplicationKey, ServiceUrl, null);
			string ticket;
			string token;
			User user;

			manager.GetTicket(out ticket);

			SubmitAuthenticationInformation(ticket);

			manager.GetAuthenticationToken(ticket, out token, out user);

			GetServerTimeResponse response = manager.GetServerTime();

			Assert.AreEqual(GetServerTimeStatus.Successful, response.Status);
			Assert.IsNull(response.UserState);
			Assert.IsNull(response.Error);
			Assert.AreNotEqual(DateTime.MinValue, response.ServerTime);
		}
	}
}
