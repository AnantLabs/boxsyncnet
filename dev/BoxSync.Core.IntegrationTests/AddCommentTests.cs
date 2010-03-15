using System;
using System.Linq;

using BoxSync.Core.Primitives;
using BoxSync.Core.Statuses;

using NUnit.Framework;


namespace BoxSync.Core.IntegrationTests
{
	/// <summary>
	/// Tests used to test add_comment web-method
	/// </summary>
	public class AddCommentTests : IntegrationTestBase
	{
		[Test]
		public void TestAddComment_Sync()
		{
			const string commentText = "djkfrbgvdjhfbgd3465346@#$%^&YU*(fjvbg dzjf  idfbgdfjkh ifbg ds";
			UploadFileResponse uploadFileResponse = UploadTemporaryFile(Context.Manager);

			AddCommentResponse addCommentResponse = Context.Manager.AddComment(uploadFileResponse.UploadedFileStatus.Keys.ElementAt(0).ID, ObjectType.File, commentText);

			DeleteTemporaryFile(Context.Manager, uploadFileResponse.UploadedFileStatus.Keys.ElementAt(0).ID);

			Assert.IsNotNull(addCommentResponse);
			Assert.AreEqual(AddCommentStatus.Successful, addCommentResponse.Status);
			Assert.AreEqual(Context.AuthenticatedUser.ID, addCommentResponse.PostedComment.UserID);
			Assert.AreEqual(0, addCommentResponse.PostedComment.ReplyComments.Count);
			Assert.AreNotEqual(DateTime.MinValue, addCommentResponse.PostedComment.CreatedOn);

			StringAssert.AreEqualIgnoringCase(commentText, addCommentResponse.PostedComment.Text);
			StringAssert.AreEqualIgnoringCase(Context.AuthenticatedUser.Login, addCommentResponse.PostedComment.UserName);
		}

		[Test]
		public void TestAddComment_Sync_WhenUserIsLoggedOut_ThenOperationStatusIsNotLoggedIn()
		{
			const string commentText = "djkfrbgvdjhfbgd3465346@#$%^&YU*(fjvbg dzjf  idfbgdfjkh ifbg ds";
			UploadFileResponse uploadFileResponse = UploadTemporaryFile(Context.Manager);

			Context.Manager.Logout();

			AddCommentResponse addCommentResponse = Context.Manager.AddComment(uploadFileResponse.UploadedFileStatus.Keys.ElementAt(0).ID, ObjectType.File, commentText);

			InitializeContext();

			DeleteTemporaryFile(Context.Manager, uploadFileResponse.UploadedFileStatus.Keys.ElementAt(0).ID);

			Assert.IsNotNull(addCommentResponse);
			Assert.AreEqual(AddCommentStatus.NotLoggedIn, addCommentResponse.Status);

			Assert.IsNull(addCommentResponse.PostedComment);
		}

		[Test]
		public void TestAddComment_Sync_WhenObjectTypeIsFolderAndObjectIDBelongsToFile_ThenOperationStatusIsFailed()
		{
			const string commentText = "djkfrbgvdjhfbgd3465346@#$%^&YU*(fjvbg dzjf  idfbgdfjkh ifbg ds";
			UploadFileResponse uploadFileResponse = UploadTemporaryFile(Context.Manager);

			AddCommentResponse addCommentResponse = Context.Manager.AddComment(uploadFileResponse.UploadedFileStatus.Keys.ElementAt(0).ID, ObjectType.Folder, commentText);

			DeleteTemporaryFile(Context.Manager, uploadFileResponse.UploadedFileStatus.Keys.ElementAt(0).ID);

			Assert.IsNotNull(addCommentResponse);
			Assert.AreEqual(AddCommentStatus.Failed, addCommentResponse.Status);
			Assert.IsNull(addCommentResponse.PostedComment);
		}

		[Test]
		public void TestAddComment_Sync_WhenCommentTextIsEmpty_ThenOperationStatusIsFailed()
		{
			string commentText = string.Empty;
			UploadFileResponse uploadFileResponse = UploadTemporaryFile(Context.Manager);

			AddCommentResponse addCommentResponse = Context.Manager.AddComment(uploadFileResponse.UploadedFileStatus.Keys.ElementAt(0).ID, ObjectType.File, commentText);

			DeleteTemporaryFile(Context.Manager, uploadFileResponse.UploadedFileStatus.Keys.ElementAt(0).ID);

			Assert.IsNotNull(addCommentResponse);
			Assert.AreEqual(AddCommentStatus.Failed, addCommentResponse.Status);
			Assert.IsNull(addCommentResponse.PostedComment);
		}

		[Test] 
		public void TestAddComment_Sync_WhenCommentTextIsNull_ThenArgumentExceptionIsThrown()
		{
			const string commentText = null;
			UploadFileResponse uploadFileResponse = UploadTemporaryFile(Context.Manager);

			try
			{
				Context.Manager.AddComment(uploadFileResponse.UploadedFileStatus.Keys.ElementAt(0).ID, ObjectType.File, commentText);
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOf(typeof(ArgumentException), ex);
			}
			finally
			{
				DeleteTemporaryFile(Context.Manager, uploadFileResponse.UploadedFileStatus.Keys.ElementAt(0).ID);
			}
		}
	}
}
