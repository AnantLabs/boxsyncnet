using System;
using System.Linq;
using System.Text;

using BoxSync.Core.Primitives;
using BoxSync.Core.Statuses;

using NUnit.Framework;


namespace BoxSync.Core.IntegrationTests
{
	[TestFixture]
	public class GetFileInfoTests : IntegrationTestBase
	{
		[Test]
		public void TestGetFileInfo()
		{
			const string fileDescription = "skdhvjbsjkdhvbfjkdshvbjdshgvfjdshv";
			byte[] fileContent = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + Guid.NewGuid());
			UploadFileResponse uploadResponse = UploadTemporaryFile(Context.Manager, fileContent, 0);

			File uploadedFileInfo = uploadResponse.UploadedFileStatus.Keys.ElementAt(0);

			Context.Manager.SetDescription(uploadedFileInfo.ID, ObjectType.File, fileDescription);
			PublicShareResponse publicShareResponse = Context.Manager.PublicShare(uploadedFileInfo.ID, ObjectType.File, null, null, new string[0]);

			GetFileInfoResponse response = Context.Manager.GetFileInfo(uploadedFileInfo.ID);

			DeleteTemporaryFile(Context.Manager, uploadedFileInfo.ID);

			Assert.IsNotNull(response);

			Assert.AreEqual(GetFileInfoStatus.Successful, response.Status);
            Assert.AreEqual(uploadedFileInfo.ID, response.File.ID);
			Assert.AreNotEqual(DateTime.MinValue, response.File.Created);
			Assert.IsTrue(response.File.IsShared.HasValue);
			Assert.IsTrue(response.File.IsShared.Value);
			StringAssert.AreEqualIgnoringCase(uploadedFileInfo.Name, response.File.Name);
			Assert.IsNull(response.File.OwnerID);
			Assert.IsNull(response.File.PermissionFlags);
			StringAssert.AreEqualIgnoringCase(publicShareResponse.PublicName, response.File.PublicName);
			Assert.IsNotNull(response.File.SHA1Hash);
			Assert.IsFalse(string.IsNullOrEmpty(response.File.SharedLink));
			Assert.AreEqual(fileContent.Length, response.File.Size);
			StringAssert.AreEqualIgnoringCase(fileDescription, response.File.Description);
		}
	}
}
