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
	[TestFixture]
	public class GetUpdatesTests : IntegrationTestBase
	{
		[Test]
		public void Test()
		{
			BoxManager manager = new BoxManager(ApplicationKey, ServiceUrl, null);
			string ticket;
			string token;
			
			manager.GetTicket(out ticket);

			SubmitAuthenticationInformation(ticket);

			DateTime fromDate = DateTime.UtcNow;
			UploadFileResponse uploadResponse = UploadTemporaryFile(manager);
			DateTime toDate = DateTime.UtcNow;

			GetUpdatesResponse getUpdatesResponse = manager.GetUpdates(fromDate, toDate, GetUpdatesOptions.None);

			Assert.IsNull(getUpdatesResponse.Error);
			Assert.IsNull(getUpdatesResponse.UserState);
			Assert.AreEqual(GetUpdatesStatus.Successful, getUpdatesResponse.Status);
		}

		public UploadFileResponse UploadTemporaryFile(BoxManager manager)
		{
			string tempFileName = Path.GetTempFileName();

			System.IO.File.WriteAllText(tempFileName, Guid.Empty.ToString());

			return manager.AddFile(tempFileName, 0);
		}

		public void DeleteTemporaryFile(BoxManager manager, long objectID)
		{
			manager.DeleteObject(objectID, ObjectType.File);
		}
	}
}
