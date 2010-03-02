using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BoxSync.Core.Primitives;
using BoxSync.Core.Statuses;
using NUnit.Framework;
using File=BoxSync.Core.Primitives.File;

namespace BoxSync.Core.IntegrationTests
{
	/// <summary>
	/// Set of tests for "GetUpdates" web method
	/// </summary>
	[TestFixture]
	public class GetUpdatesTests : IntegrationTestBase
	{
		/// <summary>
		/// Tests successful scenario of "GetUpdates" method call
		/// </summary>
		[Test]
		public void TestGetUpdates()
		{
			BoxManager manager = new BoxManager(ApplicationKey, ServiceUrl, null);
			string ticket;
			string token;
			User user;
			
			manager.GetTicket(out ticket);

			SubmitAuthenticationInformation(ticket);

			manager.GetAuthenticationToken(ticket, out token, out user);

			DateTime fromDate = manager.GetServerTime().ServerTime;
			UploadFileResponse uploadResponse = UploadTemporaryFile(manager);
			DateTime toDate = manager.GetServerTime().ServerTime;

			Assert.AreEqual(UploadFileStatus.Successful, uploadResponse.Status);

			GetUpdatesResponse getUpdatesResponse = manager.GetUpdates(fromDate, toDate, GetUpdatesOptions.NoZip);

			DeleteTemporaryFile(manager, uploadResponse.UploadedFileStatus.Keys.ToArray()[0].ID);

			Assert.IsNull(getUpdatesResponse.Error);
			Assert.IsNull(getUpdatesResponse.UserState);
			Assert.AreEqual(GetUpdatesStatus.Successful, getUpdatesResponse.Status);
		}

		private static UploadFileResponse UploadTemporaryFile(BoxManager manager)
		{
			string tempFileName = Path.GetTempFileName();

			System.IO.File.WriteAllText(tempFileName, Guid.Empty.ToString());

			return manager.AddFile(tempFileName, 0);
		}

		private static void DeleteTemporaryFile(BoxManager manager, long objectID)
		{
			manager.DeleteObject(objectID, ObjectType.File);
		}
	}
}
