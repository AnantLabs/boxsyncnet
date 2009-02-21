using System;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;

using BoxSync.Core.Primitives;
using BoxSync.Core.ServiceReference;
using BoxSync.Core.Statuses;

using ICSharpCode.SharpZipLib.Zip;


namespace BoxSync.Core
{
	/// <summary>
	/// Provides methods for using Box.NET SOAP web service
	/// </summary>
	public sealed class BoxManager
	{
		private class AuthorizationInformation
		{
			public string Ticket { get; set; }
			public string Token { get; set; }
			public SOAPUser User { get; set; }
			public string UserName { get; set; }
			public string Password { get; set; }
			public AuthorizationProcessFinished LoginFinishedCallback;
			public UpdateStatus UpdateAuthorizationStatus;
			public Action<AuthorizationInformation> CurrentOperationFinishedCallback;
			public AuthorizationStatus Status { get; set; }
		}

		private readonly boxnetService _service;
		private const string API_KEY = "36ym63ktcqhb58xn72ds6joc5yh6nozl";
		private string _token;
		private User _user;
		private readonly IWebProxy _proxy;
		private TagPrimitiveCollection _tagCollection;
		

		/// <summary>
		/// Instantiates BoxManager
		/// </summary>
		/// <param name="serviceUrl">Box.NET SOAP service Url</param>
		/// <param name="proxy">Proxy information</param>
        public BoxManager(string serviceUrl, IWebProxy proxy)
		{
			_service = new boxnetService();
			_proxy = proxy;

			_service.Url = serviceUrl;
        	_service.Proxy = proxy;
		}

		/// <summary>
		/// Instantiates BoxManager
		/// </summary>
		/// <param name="serviceUrl">Box.NET SOAP service Url</param>
		/// <param name="proxy">Proxy information</param>
		/// <param name="authorizationTocken">Valid authorization tocken</param>
		/// <param name="authorizedUser">Authorized user information</param>
		public BoxManager(string serviceUrl, IWebProxy proxy, string authorizationTocken, User authorizedUser)
		{
			_service = new boxnetService();
			_proxy = proxy;

			_service.Url = serviceUrl;
			_service.Proxy = proxy;

			_token = authorizationTocken;
			_user = authorizedUser;
		}


		/// <summary>
		/// Gets the user which is currently logged in
		/// </summary>
		public User User
		{
			get
			{
				return _user;
			}
		}


		#region Authorization methods

		/// <summary>
		/// Does user authorization
		/// </summary>
		/// <param name="accountLogin">Account login</param>
		/// <param name="accountPassword">Account password</param>
		/// <returns>Indicates if authorization process was successful</returns>
		public bool Login(string accountLogin, string accountPassword)
		{
			bool toReturn = false;
			ManualResetEvent loginFinishedEvent = new ManualResetEvent(false);

			AuthorizationProcessFinished loginFinished = loginResult =>
			                             	{
			                             		toReturn = loginResult;
			                             		loginFinishedEvent.Set();
			                             	};

			Login(accountLogin, accountPassword, loginFinished, null);

			loginFinishedEvent.WaitOne();

			return toReturn;
		}

		/// <summary>
		/// Does asynchronous user authorization
		/// </summary>
		/// <param name="userLogin">User login</param>
		/// <param name="userPassword">User password</param>
		/// <param name="authorizationFinished">Callback method which will be invoked after authorization process completes. Can't be null</param>
		/// <param name="updateAuthorizationStatus">Delegate which will be invoked on each step of authorization process. Can be null</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="authorizationFinished"/> is null</exception>
		public void Login(string userLogin, string userPassword, AuthorizationProcessFinished authorizationFinished, UpdateStatus updateAuthorizationStatus)
		{
			ThrowIfParameterIsNull(authorizationFinished, "authorizationFinished");

			AuthorizationInformation authorizationInformation = new AuthorizationInformation
			                                                    	{
			                                                    		CurrentOperationFinishedCallback = ProcessUserAuthorization,
			                                                    		LoginFinishedCallback = authorizationFinished,
			                                                    		Password = userPassword,
			                                                    		UserName = userLogin,
																		Status = AuthorizationStatus.ReadyToStartAuthorization,
			                                                    		Ticket = null,
																		UpdateAuthorizationStatus = updateAuthorizationStatus
			                                                    	};

			ProcessUserAuthorization(authorizationInformation);
		}

		/// <summary>
		/// Manages user authorization process
		/// </summary>
		/// <param name="authorizationInformation">Authorization information</param>
		private void ProcessUserAuthorization(AuthorizationInformation authorizationInformation)
		{
			switch (authorizationInformation.Status)
			{
				case AuthorizationStatus.ReadyToStartAuthorization:
					authorizationInformation.UpdateAuthorizationStatus.SafeInvoke("Ready to start authorization...");
					GetTicket(authorizationInformation);
					authorizationInformation.UpdateAuthorizationStatus.SafeInvoke("Retrieving ticket...");
					break;
				case AuthorizationStatus.GetTicketFinishedSuccessful:
					authorizationInformation.UpdateAuthorizationStatus.SafeInvoke("Submiting login/password...");
					SubmitAuthorizationInformation(authorizationInformation);
					ProcessUserAuthorization(authorizationInformation);
					break;
				case AuthorizationStatus.SubmitUserCredentialsFinishedSuccessful:
					authorizationInformation.UpdateAuthorizationStatus.SafeInvoke("Retrieving authorization token...");
					GetAuthorizationToken(authorizationInformation);
					break;
				case AuthorizationStatus.GetAuthorizationTokenFinishedSuccessful:
					authorizationInformation.UpdateAuthorizationStatus.SafeInvoke("Authorization finished successfuly...");
					authorizationInformation.Status = AuthorizationStatus.AuthorizationFinishedSuccessfuly;

					_token = authorizationInformation.Token;
					_user = new User(authorizationInformation.User);

					authorizationInformation.LoginFinishedCallback(true);
					break;

				case AuthorizationStatus.GetAuthorizationTokenFailed:
					authorizationInformation.UpdateAuthorizationStatus.SafeInvoke("Failed to retrieve authorization tocken...");
					authorizationInformation.LoginFinishedCallback(false);
					break;
				case AuthorizationStatus.GetTicketFinishedFailed:
					authorizationInformation.UpdateAuthorizationStatus.SafeInvoke("Failed to retrieve user ticket...");
					authorizationInformation.LoginFinishedCallback(false);
					break;
				case AuthorizationStatus.SubmitUserCredentialsFailed:
					authorizationInformation.UpdateAuthorizationStatus.SafeInvoke("Failed submit login/password...");
					authorizationInformation.LoginFinishedCallback(false);
					break;
			}

			
		}

		/// <summary>
		/// Starts asynchronous retrieving of authorization ticket
		/// </summary>
		/// <param name="authorizationInformation">Authorization information</param>
		private void GetTicket(AuthorizationInformation authorizationInformation)
		{
			_service.Beginget_ticket(API_KEY, GetTicketFinished, authorizationInformation);
		}
		
		/// <summary>
		/// Finishes asynchronous retrieving of authorization ticket
		/// </summary>
		/// <param name="asyncResult">Result of asynchronous execution</param>
		private void GetTicketFinished(IAsyncResult asyncResult)
		{
			string ticket;
			string result = _service.Endget_ticket(asyncResult, out ticket);

			GetTicketStatus status = StatusMessageParser.ParseGetTicketStatus(result);
			AuthorizationInformation authorizationInformation = (AuthorizationInformation)asyncResult.AsyncState;

			if (!string.IsNullOrEmpty(ticket) && status == GetTicketStatus.Successful)
			{
				authorizationInformation.Status = AuthorizationStatus.GetTicketFinishedSuccessful;
				authorizationInformation.Ticket = ticket;
			}
			else
			{
				authorizationInformation.Status = AuthorizationStatus.GetTicketFinishedFailed;
			}

			authorizationInformation.CurrentOperationFinishedCallback(authorizationInformation);
		}


		/// <summary>
		/// Starts asynchronous retrieving of authorization token
		/// </summary>
		/// <param name="authorizationInformation">Authorization information</param>
		private void GetAuthorizationToken(AuthorizationInformation authorizationInformation)
		{
			_service.Beginget_auth_token(API_KEY, authorizationInformation.Ticket, GetAuthorizationTokenFinished,
			                             authorizationInformation);
		}
		
		/// <summary>
		/// Finishes asynchronous retrieving of authorization token
		/// </summary>
		/// <param name="asyncResult">Result of asynchronous execution</param>
		public void GetAuthorizationTokenFinished(IAsyncResult asyncResult)
		{
			string token;
			SOAPUser user;

			string result = _service.Endget_auth_token(asyncResult, out token, out user);

			GetAuthorizationTockenStatus status = StatusMessageParser.ParseGetAuthorizationTockenStatus(result);

			AuthorizationInformation authorizationInformation = (AuthorizationInformation)asyncResult.AsyncState;

			if (status != GetAuthorizationTockenStatus.Successful || string.IsNullOrEmpty(token))
			{
				authorizationInformation.Status = AuthorizationStatus.GetAuthorizationTokenFailed;
			}
			else
			{
				authorizationInformation.Status = AuthorizationStatus.GetAuthorizationTokenFinishedSuccessful;
				authorizationInformation.Token = token;
				authorizationInformation.User = user;
			}

			authorizationInformation.CurrentOperationFinishedCallback(authorizationInformation);
		}


		/// <summary>
		/// Submits login/password
		/// </summary>
		/// <param name="authorizationInformation">Authorization information</param>
		/// <returns>Result which server returns after login/password submit</returns>
		private string SubmitAuthorizationInformation(AuthorizationInformation authorizationInformation)
		{
			string uploadResult = null;

			using (WebClient client = new WebClient {Proxy = _proxy})
			{

				client.Headers.Add("Content-Type:application/x-www-form-urlencoded");

				Uri destinationAddress = new Uri("http://www.box.net/api/1.0/auth/" + authorizationInformation.Ticket);

				ManualResetEvent submitFinishedEvent = new ManualResetEvent(false);

				Action submitLoginPassword = () =>
				                             	{
				                             		uploadResult = client.UploadString(destinationAddress, "POST",
				                             		                                   "login=" + authorizationInformation.UserName +
				                             		                                   "&password=" +
				                             		                                   authorizationInformation.Password +
				                             		                                   "&dologin=1&__login=1");
				                             	};

				AsyncCallback callback = asyncResult =>
				                         	{
				                         		ManualResetEvent submitFinished = (ManualResetEvent) asyncResult.AsyncState;

				                         		submitFinished.Set();
				                         	};

				IAsyncResult asyncResult2 = submitLoginPassword.BeginInvoke(callback, submitFinishedEvent);

				submitFinishedEvent.WaitOne();

				submitLoginPassword.EndInvoke(asyncResult2);
			}

			authorizationInformation.Status = !string.IsNullOrEmpty(uploadResult)
			                                  	? AuthorizationStatus.SubmitUserCredentialsFinishedSuccessful
			                                  	: AuthorizationStatus.SubmitUserCredentialsFailed;

			return uploadResult;
		}
		
		#endregion

		#region Upload file

		/// <summary>
		/// Uploads the specified local file to the specified folder
		/// </summary>
		/// <param name="filePath">Path to the file which needs to be uploaded</param>
		/// <param name="destinationFolderID">ID of the destination folder</param>
		/// <returns>Operation status</returns>
		public UploadFileResponse UploadFile(string filePath, long destinationFolderID)
		{
			UploadFileResponse uploadFileResponse;
			
			using (WebClient client = new WebClient { Proxy = _proxy })
			{
				Uri destinationAddress = new Uri(string.Format("http://upload.box.net/api/1.0/upload/{0}/{1}", _token, destinationFolderID));

				byte[] response = client.UploadFile(destinationAddress, "POST", filePath);
				
				string result = Encoding.ASCII.GetString(response);

				uploadFileResponse = MessageParser.Instance.ParseUploadResponseMessage(result);
				uploadFileResponse.FolderID = destinationFolderID;
			}

			return uploadFileResponse;
		}

		/// <summary>
		/// Asynchronously uploads the specified local file to the specified folder
		/// </summary>
		/// <param name="filePath">Path to the file which needs to be uploaded</param>
		/// <param name="parentFolderID">ID of the destination folder</param>
		/// <param name="fileUploadCompleted">Callback method which will be invoked after file-upload operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="fileUploadCompleted"/> is null</exception>
		public void UploadFile(string filePath, long parentFolderID, OperationFinished<UploadFileStatus, UploadFileResponse> fileUploadCompleted)
		{
			ThrowIfParameterIsNull(fileUploadCompleted, "fileUploadCompleted");

			using (WebClient client = new WebClient { Proxy = _proxy })
			{
				Uri destinationAddress = new Uri(string.Format("http://upload.box.net/api/1.0/upload/{0}/{1}", _token, parentFolderID));

				client.UploadFileCompleted += UploadFileFinished;

				object[] state = new object[2];

				state[0] = parentFolderID;
				state[1] = fileUploadCompleted;

				client.UploadFileAsync(destinationAddress, "POST", filePath, state);
			}
		}
		
		/// <summary>
		/// Handler method which will be executed after file-upload operation completes
		/// </summary>
		/// <param name="sender">The source of the event</param>
		/// <param name="e">Argument that contains event data</param>
		private void UploadFileFinished(object sender, UploadFileCompletedEventArgs e)
		{
			object[] state = (object[])e.UserState;
			long folderID = (long)state[0];
			OperationFinished<UploadFileStatus, UploadFileResponse> fileUploadFinishedHandler = (OperationFinished<UploadFileStatus, UploadFileResponse>)state[1];

			if (e.Cancelled)
			{
				fileUploadFinishedHandler(UploadFileStatus.Cancelled, null, null);
				return;
			}

			string result = Encoding.ASCII.GetString(e.Result);

			UploadFileResponse uploadFileResponse = MessageParser.Instance.ParseUploadResponseMessage(result);

			uploadFileResponse.FolderID = folderID;

			fileUploadFinishedHandler(uploadFileResponse.Status, uploadFileResponse, null);
		}
		#endregion

		#region Create folder
		
		/// <summary>
		/// Creates folder
		/// </summary>
		/// <param name="folderName">Folder name</param>
		/// <param name="parentFolderID">ID of the parent folder where new folder needs to be created or '0'</param>
		/// <param name="isShared">Indicates if new folder will be publicly shared</param>
		/// <param name="folder">Contains all information about newly created folder</param>
		/// <returns>Operation status</returns>
		public CreateFolderStatus CreateFolder(string folderName, long parentFolderID, bool isShared, out FolderBase folder)
		{
			SOAPFolder soapFolder;
			string response = _service.create_folder(API_KEY, _token, parentFolderID, folderName, isShared ? 1 : 0, out soapFolder);
			
			folder = new FolderBase(soapFolder);

			return StatusMessageParser.ParseAddFolderStatus(response);
		}

		/// <summary>
		/// Asynchronously creates folder
		/// </summary>
		/// <param name="folderName">Folder name</param>
		/// <param name="parentFolderID">ID of the parent folder where new folder needs to be created or '0'</param>
		/// <param name="isShared">Indicates if new folder will be publicly shared</param>
		/// <param name="createFolderCompleted">Callback method which will be invoked after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="createFolderCompleted"/> is null</exception>
		public void CreateFolder(string folderName, long parentFolderID, bool isShared, OperationFinished<CreateFolderStatus, FolderBase> createFolderCompleted)
		{
			ThrowIfParameterIsNull(createFolderCompleted, "createFolderCompleted");

			_service.create_folderCompleted += CreateFolderFinished;

			_service.create_folderAsync(API_KEY, _token, parentFolderID, folderName, isShared ? 1 : 0, createFolderCompleted);
		}

		private void CreateFolderFinished(object sender, create_folderCompletedEventArgs e)
		{
			OperationFinished<CreateFolderStatus, FolderBase> createFolderFinishedHandler =
				(OperationFinished<CreateFolderStatus, FolderBase>)e.UserState;

			CreateFolderStatus status = StatusMessageParser.ParseAddFolderStatus(e.Result);
			FolderBase folder = new FolderBase(e.folder);

			switch (status)
			{
				case CreateFolderStatus.Successful:
				case CreateFolderStatus.ApplicationRestricted:
				case CreateFolderStatus.NoParentFolder:
				case CreateFolderStatus.NotLoggedIn:
					createFolderFinishedHandler(status, folder, null);
					break;
				default:
					createFolderFinishedHandler(status, folder, e.Result);
					break;
			}
		}
		
		#endregion

		#region Delete object

		/// <summary>
		/// Deletes specified object
		/// </summary>
		/// <param name="objectID">ID of the object to delete</param>
		/// <param name="objectType">Type of the object</param>
		/// <returns>Operation status</returns>
		public DeleteObjectStatus DeleteObject(long objectID, ObjectType objectType)
		{
			string type = ObjectType2String(objectType);
			string result = _service.delete(API_KEY, _token, type, objectID);

			return StatusMessageParser.ParseDeleteObjectStatus(result);
		}
		
		/// <summary>
		/// Asynchronously deletes specified object
		/// </summary>
		/// <param name="objectID">ID of the object to delete</param>
		/// <param name="objectType">Type of the object</param>
		/// <param name="deleteObjectCompleted">Callback method which will be executed after delete operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="deleteObjectCompleted"/> is null</exception>
		public void DeleteObject(long objectID, ObjectType objectType, OperationFinished<DeleteObjectStatus> deleteObjectCompleted)
		{
			ThrowIfParameterIsNull(deleteObjectCompleted, "deleteObjectCompleted");

			string type = ObjectType2String(objectType);

			_service.deleteCompleted += DeleteObjectFinished;

			_service.deleteAsync(API_KEY, _token, type, objectID, deleteObjectCompleted);
		}

		private void DeleteObjectFinished(object sender, deleteCompletedEventArgs e)
		{
			OperationFinished<DeleteObjectStatus> deleteObjectFinishedHandler =
				(OperationFinished<DeleteObjectStatus>)e.UserState;

			DeleteObjectStatus status = StatusMessageParser.ParseDeleteObjectStatus(e.Result);

			switch (status)
			{
				case DeleteObjectStatus.Successful:
				case DeleteObjectStatus.Failed:
				case DeleteObjectStatus.ApplicationRestricted:
				case DeleteObjectStatus.NotLoggedIn:
					deleteObjectFinishedHandler(status, null);
					break;
				default:
					deleteObjectFinishedHandler(status, e.Result);
					break;
			}
		}
		
		#endregion

		#region GetFolderStructure

		/// <summary>
		/// Retrieves a user's root folder structure
		/// </summary>
		/// <param name="retrieveOptions">Retrieve options</param>
		/// <param name="folder">Root folder</param>
		/// <returns>Operation status</returns>
		public GetAccountTreeStatus GetRootFolderStructure(RetrieveFolderStructureOptions retrieveOptions, out Folder folder)
		{
			return GetFolderStructure(0, retrieveOptions, out folder);
		}

		/// <summary>
		/// Asynchronously retrieves a user's root folder structure
		/// </summary>
		/// <param name="retrieveOptions">Retrieve options</param>
		/// <param name="getFolderStructureCompleted">Callback method which will be executed after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="getFolderStructureCompleted"/> is null</exception>
		public void GetRootFolderStructure(RetrieveFolderStructureOptions retrieveOptions, OperationFinished<GetAccountTreeStatus, Folder> getFolderStructureCompleted)
		{
			ThrowIfParameterIsNull(getFolderStructureCompleted, "getFolderStructureCompleted");

			GetFolderStructure(0, retrieveOptions, getFolderStructureCompleted);
		}

		/// <summary>
		/// Retrieves a user's folder structure by ID
		/// </summary>
		/// <param name="folderID">ID of the folder to retrieve</param>
		/// <param name="retrieveOptions">Retrieve options</param>
		/// <param name="folder">Folder object</param>
		/// <returns>Operation status</returns>
		public GetAccountTreeStatus GetFolderStructure(long folderID, RetrieveFolderStructureOptions retrieveOptions, out Folder folder)
		{
			folder = null;

			byte[] folderInfoXml;

			string result = _service.get_account_tree(API_KEY, _token, folderID, new string[0], out folderInfoXml);
			GetAccountTreeStatus status = StatusMessageParser.ParseGetAccountTreeStatus(result);

			switch (status)
			{
				case GetAccountTreeStatus.Successful:
					string folderInfo = null;

					if (!retrieveOptions.Contains(RetrieveFolderStructureOptions.NoZip))
					{
						folderInfoXml = Unzip(folderInfoXml);
					}

					if (folderInfoXml != null)
					{
						folderInfo = Encoding.ASCII.GetString(folderInfoXml);
					}

					folder = ParseFolderStructureXmlMessage(folderInfo);
					break;
			}

			return status;
		}

		/// <summary>
		/// Asynchronously retrieves a user's folder structure by ID
		/// </summary>
		/// <param name="folderID">ID of the folder to retrieve</param>
		/// <param name="retrieveOptions">Retrieve options</param>
		/// <param name="getFolderStructureCompleted">Callback method which will be executed after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="getFolderStructureCompleted"/> is null</exception>
		public void GetFolderStructure(long folderID, RetrieveFolderStructureOptions retrieveOptions, OperationFinished<GetAccountTreeStatus, Folder> getFolderStructureCompleted)
		{
			ThrowIfParameterIsNull(getFolderStructureCompleted, "getFolderStructureCompleted");

			object[] state = new object[2];

			state[0] = retrieveOptions;
			state[1] = getFolderStructureCompleted;

			_service.get_account_treeCompleted += GetFolderStructureFinished;

			_service.get_account_treeAsync(API_KEY, _token, folderID, retrieveOptions.ToStringArray(), state);
		}

		private void GetFolderStructureFinished(object sender, get_account_treeCompletedEventArgs e)
		{
			object[] state = (object[])e.UserState;
			RetrieveFolderStructureOptions retrieveOptions = (RetrieveFolderStructureOptions)state[0];
			OperationFinished<GetAccountTreeStatus, Folder> getFolderStructureFinishedHandler = (OperationFinished<GetAccountTreeStatus, Folder>)state[1];

			GetAccountTreeStatus status = StatusMessageParser.ParseGetAccountTreeStatus(e.Result);
			
			switch (status)
			{
				case GetAccountTreeStatus.Successful:
					byte[] folderInfoXml = null;
					string folderInfo = null;

					if (!retrieveOptions.Contains(RetrieveFolderStructureOptions.NoZip))
					{
						folderInfoXml = Unzip(e.tree);
					}

					if (folderInfoXml != null)
					{
						folderInfo = Encoding.ASCII.GetString(folderInfoXml);
					}

					Folder folder = ParseFolderStructureXmlMessage(folderInfo);

					getFolderStructureFinishedHandler(status, folder, null);
					break;
				case GetAccountTreeStatus.ApplicationRestricted:
				case GetAccountTreeStatus.FolderIDError:
				case GetAccountTreeStatus.NotLoggedID:
					getFolderStructureFinishedHandler(status, null, null);
					break;
				default:
					getFolderStructureFinishedHandler(status, null, e.Result);
					break;
			}
		}
		
		#endregion

		#region ExportTags
		
		/// <summary>
		/// Retrieves list of user's tags
		/// </summary>
		/// <param name="tagList">List of user's tags</param>
		/// <returns>Operation status</returns>
		public ExportTagsStatus ExportTags(out TagPrimitiveCollection tagList)
		{
			byte[] xmlMessage;

			string result = _service.export_tags(API_KEY, _token, out xmlMessage);
			ExportTagsStatus status = StatusMessageParser.ParseExportTagStatus(result);

			tagList = MessageParser.Instance.ParseExportTagsMessage(Encoding.ASCII.GetString(xmlMessage));

			return status;
		}

		/// <summary>
		/// Asynchronously retrieves list of user's tags
		/// </summary>
		/// <param name="exportTagsCompleted">Callback method which will be invioked after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="exportTagsCompleted"/> is null</exception>
		public void ExportTags(OperationFinished<ExportTagsStatus, TagPrimitiveCollection> exportTagsCompleted)
		{
			ThrowIfParameterIsNull(exportTagsCompleted, "exportTagsCompleted");

			_service.export_tagsCompleted += ExportTagsFinished;

			_service.export_tagsAsync(API_KEY, _token, exportTagsCompleted);
		}

		private void ExportTagsFinished(object sender, export_tagsCompletedEventArgs e)
		{
			OperationFinished<ExportTagsStatus, TagPrimitiveCollection> exportTagsFinishedHandler =
				(OperationFinished<ExportTagsStatus, TagPrimitiveCollection>)e.UserState;

			ExportTagsStatus status = StatusMessageParser.ParseExportTagStatus(e.Result);

			switch (status)
			{
				case ExportTagsStatus.Successful:
					TagPrimitiveCollection tags = MessageParser.Instance.ParseExportTagsMessage(Encoding.ASCII.GetString(e.tag_xml));
					exportTagsFinishedHandler(status, tags, null);
					break;
				case ExportTagsStatus.ApplicationRestricted:
				case ExportTagsStatus.NotLoggedID:
					exportTagsFinishedHandler(status, new TagPrimitiveCollection(), null);
					break;
				default:
					exportTagsFinishedHandler(status, null, e.Result);
					break;
			}
		}
		
		#endregion

		#region GetTag
		private TagPrimitive GetTag(long id)
		{
			ManualResetEvent wait = new ManualResetEvent(false);
			TagPrimitive result = new TagPrimitive();

			OperationFinished<ExportTagsStatus, TagPrimitive> getTagFinishedHandler = (status, tag, errorData) =>
			                                                                          	{
			                                                                          		result = tag;
			                                                                          		wait.Reset();
			                                                                          	};
			GetTag(id, getTagFinishedHandler);
			wait.WaitOne();

			return result;
		}
		private void GetTag(long id, OperationFinished<ExportTagsStatus, TagPrimitive> getTagFinishedHandler)
		{
			if (_tagCollection == null || _tagCollection.IsEmpty)
			{
				OperationFinished<ExportTagsStatus, TagPrimitiveCollection> exportTagsFinishedHandler =
					(status, tags, errorData) =>
						{
							_tagCollection = tags;

							getTagFinishedHandler(status, _tagCollection.GetTag(id), errorData);
						};

				ExportTags(exportTagsFinishedHandler);
			}
			else
			{
				getTagFinishedHandler(ExportTagsStatus.Successful, _tagCollection.GetTag(id), null);
			}
		}
		#endregion

		#region SetDescription

		/// <summary>
		/// Sets description of the object
		/// </summary>
		/// <param name="objectID">ID of the object</param>
		/// <param name="objectType">Object type</param>
		/// <param name="description">Description text</param>
		/// <returns>Operation status</returns>
		public SetDescriptionStatus SetDescription(long objectID, ObjectType objectType, string description)
		{
			string type = ObjectType2String(objectType);

			string result = _service.set_description(API_KEY, _token, type, objectID, description);
			
			return StatusMessageParser.ParseSetDescriptionStatus(result);
		}

		/// <summary>
		/// Asynchronously sets description of the object
		/// </summary>
		/// <param name="objectID">ID of the object</param>
		/// <param name="objectType">Object type</param>
		/// <param name="description">Description text</param>
		/// <param name="setDescriptionCompleted">Callback method which will be invoked after delete operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="setDescriptionCompleted"/> is null</exception>
		public void SetDescription(long objectID, ObjectType objectType, string description, OperationFinished<SetDescriptionStatus> setDescriptionCompleted)
		{
			ThrowIfParameterIsNull(setDescriptionCompleted, "setDescriptionCompleted");

			string type = ObjectType2String(objectType);

			_service.set_descriptionCompleted += SetDescriptionFinished;

			_service.set_descriptionAsync(API_KEY, _token, type, objectID, description, setDescriptionCompleted);
		}

		private void SetDescriptionFinished(object sender, set_descriptionCompletedEventArgs e)
		{
			OperationFinished<SetDescriptionStatus> setDescriptionFinishedHandler = (OperationFinished<SetDescriptionStatus>)e.UserState;

			SetDescriptionStatus status = StatusMessageParser.ParseSetDescriptionStatus(e.Result);

			switch (status)
			{
				case SetDescriptionStatus.Failed:
				case SetDescriptionStatus.Successful:
					setDescriptionFinishedHandler(status, null);
					break;
				default:
					setDescriptionFinishedHandler(status, e.Result);
					break;
			}
		}

		#endregion

		#region Rename

		/// <summary>
		/// Renames specified object
		/// </summary>
		/// <param name="objectID">ID of the object which needs to be renamed</param>
		/// <param name="objectType">Type of the object</param>
		/// <param name="newName">New name of the object</param>
		/// <returns>Operation status</returns>
		public RenameObjectStatus RenameObject(long objectID, ObjectType objectType, string newName)
		{
			string type = ObjectType2String(objectType);
			string result = _service.rename(API_KEY, _token, type, objectID, newName);

			return StatusMessageParser.ParseRenameObjectStatus(result);
		}
		
		/// <summary>
		/// Asynchronously renames specified object
		/// </summary>
		/// <param name="objectID">ID of the object which needs to be renamed</param>
		/// <param name="objectType">Type of the object</param>
		/// <param name="newName">New name of the object</param>
		/// <param name="renameObjectCompleted">Callback method which will be invoked after rename operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="renameObjectCompleted"/> is null</exception>
		public void RenameObject(long objectID, ObjectType objectType, string newName, OperationFinished<RenameObjectStatus> renameObjectCompleted)
		{
			ThrowIfParameterIsNull(renameObjectCompleted, "renameObjectCompleted");

			string type = ObjectType2String(objectType);
			
			_service.renameCompleted += RenameObjectCompleted;

			_service.renameAsync(API_KEY, _token, type, objectID, newName, renameObjectCompleted);
		}

		/// <summary>
		/// Handler method which will be executed after rename operation completes
		/// </summary>
		/// <param name="sender">The source of the event</param>
		/// <param name="e">Argument that contains event data</param>
		private void RenameObjectCompleted(object sender, renameCompletedEventArgs e)
		{
			OperationFinished<RenameObjectStatus> renameObjectFinishedHandler = (OperationFinished<RenameObjectStatus>)e.UserState;

			if(e.Cancelled)
			{
				renameObjectFinishedHandler(RenameObjectStatus.Failed, null);
			}

			RenameObjectStatus status = StatusMessageParser.ParseRenameObjectStatus(e.Result);
			string errorData = status == RenameObjectStatus.Unknown ? e.Result : null;
				
			renameObjectFinishedHandler(status, errorData);
		}
		
		#endregion

		#region Copy

		/// <summary>
		/// Copies file or folder to a specified folder
		/// (currently works only for files)
		/// </summary>
		/// <param name="targetObjectID">ID of the object which must be copied</param>
		/// <param name="targetObjectType">Type of the object which must be copied</param>
		/// <param name="destinationFolderID">ID of the destination folder</param>
		/// <returns>Operation status</returns>
		public CopyObjectStatus CopyObject(long targetObjectID, ObjectType targetObjectType, long destinationFolderID)
		{
			string type = ObjectType2String(targetObjectType);
			string result = _service.copy(API_KEY, _token, type, targetObjectID, destinationFolderID);

			return StatusMessageParser.ParseCopyObjectStatus(result);
		}
		
		/// <summary>
		/// Asynchronously copies file or folder to a specified folder
		/// </summary>
		/// <param name="targetObjectID">ID of the object which must be copied</param>
		/// <param name="targetObjectType">Type of the object which must be copied</param>
		/// <param name="destinationFolderID">ID of the destination folder</param>
		/// <param name="copyObjectCompleted">Callback method which will be invoked after copy operation completes</param>
		public void CopyObject(long targetObjectID, ObjectType targetObjectType, long destinationFolderID, OperationFinished<CopyObjectStatus> copyObjectCompleted)
		{
			string type = ObjectType2String(targetObjectType);

			_service.copyCompleted += CopyObjectFinished;

			_service.copyAsync(API_KEY, _token, type, targetObjectID, destinationFolderID, copyObjectCompleted);
		}

		private void CopyObjectFinished(object sender, copyCompletedEventArgs e)
		{
			OperationFinished<CopyObjectStatus> copyObjectFinishedHandler =
				(OperationFinished<CopyObjectStatus>)e.UserState;

			CopyObjectStatus status = StatusMessageParser.ParseCopyObjectStatus(e.Result);

			switch (status)
			{
				case CopyObjectStatus.Successful:
				case CopyObjectStatus.Failed:
					copyObjectFinishedHandler(status, null);
					break;
				default:
					copyObjectFinishedHandler(status, e.Result);
					break;
			}
		}
		
		#endregion

		#region Move
		
		/// <summary>
		/// Moves object from one folder to another one
		/// </summary>
		/// <param name="targetObjectID">ID of the object which needs to be moved</param>
		/// <param name="targetObjectType">Type of the object</param>
		/// <param name="destinationFolderID">ID of the destination folder</param>
		/// <returns>Operation status</returns>
		public MoveObjectStatus MoveObject(long targetObjectID, ObjectType targetObjectType, long destinationFolderID)
		{
			string type = ObjectType2String(targetObjectType);
			string result = _service.move(API_KEY, _token, type, targetObjectID, destinationFolderID);

			return StatusMessageParser.ParseMoveObjectStatus(result);
		}
		
		/// <summary>
		/// Asynchronously moves object from one folder to another one
		/// </summary>
		/// <param name="targetObjectID">ID of the object which needs to be moved</param>
		/// <param name="targetObjectType">Type of the object</param>
		/// <param name="destinationFolderID">ID of the destination folder</param>
		/// <param name="moveObjectCompleted">Callback method which will be invoked after move operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="moveObjectCompleted"/> is null</exception>
		public void MoveObject(long targetObjectID, ObjectType targetObjectType, long destinationFolderID, OperationFinished<MoveObjectStatus> moveObjectCompleted)
		{
			ThrowIfParameterIsNull(moveObjectCompleted, "moveObjectCompleted");

			string type = ObjectType2String(targetObjectType);

			_service.moveCompleted += MoveObjectFinished;

			_service.moveAsync(API_KEY, _token, type, targetObjectID, destinationFolderID, moveObjectCompleted);
		}

		private void MoveObjectFinished(object sender, moveCompletedEventArgs e)
		{
			OperationFinished<MoveObjectStatus> moveObjectFinishedHandler = (OperationFinished<MoveObjectStatus>)e.UserState;

			MoveObjectStatus status = StatusMessageParser.ParseMoveObjectStatus(e.Result);

			switch (status)
			{
				case MoveObjectStatus.Successful:
				case MoveObjectStatus.ApplicationRestricted:
				case MoveObjectStatus.Failed:
				case MoveObjectStatus.NotLoggedIn:
					moveObjectFinishedHandler(status, null);
					break;
				default:
					moveObjectFinishedHandler(status, e.Result);
					break;
			}
		}
		
		#endregion

		#region Logout

		/// <summary>
		/// Logouts current user
		/// </summary>
		/// <returns>Operation status</returns>
		public LogoutStatus Logout()
		{
			string result = _service.logout(API_KEY, _token);

			return StatusMessageParser.ParseLogoutStatus(result);
		}

		/// <summary>
		/// Asynchronously logouts current user
		/// </summary>
		/// <param name="logoutCompleted">Callback method which will be invoked after logout operation completes</param>
		public void Logout(OperationFinished<LogoutStatus> logoutCompleted)
		{
			ThrowIfParameterIsNull(logoutCompleted, "logoutCompleted");

			_service.logoutCompleted += LogoutFinished;

			_service.logoutAsync(API_KEY, _token, logoutCompleted);
		}

		private void LogoutFinished(object sender, logoutCompletedEventArgs e)
		{
			OperationFinished<LogoutStatus> logoutFinishedHandler = (OperationFinished<LogoutStatus>)e.UserState;

			LogoutStatus status = StatusMessageParser.ParseLogoutStatus(e.Result);

			switch (status)
			{
				case LogoutStatus.Successful:
				case LogoutStatus.InvalidAuthToken:
					logoutFinishedHandler(status, null);
					break;
				case LogoutStatus.Unknown:
					logoutFinishedHandler(status, e.Result);
					break;
			}
		}
		#endregion

		#region RegisterNewUser

		/// <summary>
		/// Registers new Box.NET user
		/// </summary>
		/// <param name="login">Account login name</param>
		/// <param name="password">Account password</param>
		/// <param name="response">Contains information about user account and valid authorization token</param>
		/// <returns>Operation status</returns>
		public RegisterNewUserStatus RegisterNewUser(string login, string password, out RegisterNewUserResponse response)
		{
			string token;
			SOAPUser user;

			string result = _service.register_new_user(API_KEY, login, password, out token, out user);

			response = new RegisterNewUserResponse {Token = token, User = user};

			return StatusMessageParser.ParseRegisterNewUserStatus(result);
		}
		
		/// <summary>
		/// Asynchronously registers new Box.NET user
		/// </summary>
		/// <param name="login">Account login name</param>
		/// <param name="password">Account password</param>
		/// <param name="registerNewUserCompleted">Callback method which will be invoked after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="registerNewUserCompleted"/> is null</exception>
		public void RegisterNewUser(string login, string password, OperationFinished<RegisterNewUserStatus, RegisterNewUserResponse> registerNewUserCompleted)
		{
			ThrowIfParameterIsNull(registerNewUserCompleted, "registerNewUserCompleted");

			_service.register_new_userCompleted += RegisterNewUserFinished;

			_service.register_new_userAsync(API_KEY, login, password, registerNewUserCompleted);
		}

		private void RegisterNewUserFinished(object sender, register_new_userCompletedEventArgs e)
		{
			RegisterNewUserResponse response = new RegisterNewUserResponse {Token = e.auth_token, User = e.user};
			RegisterNewUserStatus status = StatusMessageParser.ParseRegisterNewUserStatus(e.Result);
			
			OperationFinished<RegisterNewUserStatus, RegisterNewUserResponse> registerNewUserFinishedHandler =
				(OperationFinished<RegisterNewUserStatus, RegisterNewUserResponse>)e.UserState;

			switch (status)
			{
				case RegisterNewUserStatus.Successful:
				case RegisterNewUserStatus.ApplicationRestricted:
				case RegisterNewUserStatus.EmailAlreadyRegistered:
				case RegisterNewUserStatus.EmailInvalid:
				case RegisterNewUserStatus.Failed:
					registerNewUserFinishedHandler(status, response, null);
					break;
				default:
					registerNewUserFinishedHandler(status, response, e.Result);
					break;
			}
		}

		#endregion

		#region VerifyRegistrationEmail

		/// <summary>
		/// Verifies registration email address
		/// </summary>
		/// <param name="login">Account login name</param>
		/// <returns>Operation status</returns>
		public VerifyRegistrationEmailStatus VerifyRegistrationEmail(string login)
		{
			string result = _service.verify_registration_email(API_KEY, login);

			return StatusMessageParser.ParseVerifyRegistrationEmailStatus(result);
		}

		/// <summary>
		/// Asynchronously verifies registration email address
		/// </summary>
		/// <param name="login">Account login name</param>
		/// <param name="verifyRegistrationEmailCompleted">Callback method which will be invoked after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="verifyRegistrationEmailCompleted"/> is null</exception>
		public void VerifyRegistrationEmail(string login, OperationFinished<VerifyRegistrationEmailStatus> verifyRegistrationEmailCompleted)
		{
			ThrowIfParameterIsNull(verifyRegistrationEmailCompleted, "verifyRegistrationEmailCompleted");

			_service.verify_registration_emailCompleted += VerifyRegistrationEmail;

			_service.verify_registration_emailAsync(API_KEY, login, verifyRegistrationEmailCompleted);
		}

		private void VerifyRegistrationEmail(object sender, verify_registration_emailCompletedEventArgs e)
		{
			VerifyRegistrationEmailStatus status = StatusMessageParser.ParseVerifyRegistrationEmailStatus(e.Result);
			OperationFinished<VerifyRegistrationEmailStatus> verifyRegistrationEmailFinishedHandler = (OperationFinished<VerifyRegistrationEmailStatus>)e.UserState;

			switch (status)
			{
				case VerifyRegistrationEmailStatus.EmailOK:
				case VerifyRegistrationEmailStatus.ApplicationRestricted:
				case VerifyRegistrationEmailStatus.EmailInvalid:
				case VerifyRegistrationEmailStatus.EmailAlreadyRegistered:
					verifyRegistrationEmailFinishedHandler(status, null);
					break;
				default:
					verifyRegistrationEmailFinishedHandler(status, e.Result);
					break;
			}
		}
		
		#endregion

		#region AddToMyBox

		/// <summary>
		/// Copies a file publicly shared by someone to a user's folder
		/// </summary>
		/// <param name="targetFileID">ID of the file which needs to be copied</param>
		/// <param name="destinationFolderID">ID of the destination folder</param>
		/// <param name="tagList">Tags which need to be assigned to the target file</param>
		/// <returns>Operation status</returns>
		public AddToMyBoxStatus AddToMyBox(long targetFileID, long destinationFolderID, TagPrimitiveCollection tagList)
		{
			string result = _service.add_to_mybox(API_KEY, _token, targetFileID, null, destinationFolderID,
			                      ConvertTagPrimitiveCollection2String(tagList));

			return StatusMessageParser.ParseAddToMyBoxStatus(result);
		}

		/// <summary>
		/// Copies a file publicly shared by someone to a user's folder
		/// </summary>
		/// <param name="targetFileName">Name of the file which needs to be copied</param>
		/// <param name="destinationFolderID">ID of the destination folder</param>
		/// <param name="tagList">Tags which need to be assigned to the target file</param>
		/// <returns>Operation status</returns>
		public AddToMyBoxStatus AddToMyBox(string targetFileName, long destinationFolderID, TagPrimitiveCollection tagList)
		{
			string result = _service.add_to_mybox(API_KEY, _token, 0, targetFileName, destinationFolderID,
								  ConvertTagPrimitiveCollection2String(tagList));

			return StatusMessageParser.ParseAddToMyBoxStatus(result);
		}

		/// <summary>
		/// Asuncronously copies a file publicly shared by someone to a user's folder
		/// </summary>
		/// <param name="targetFileID">ID of the file which needs to be copied</param>
		/// <param name="destinationFolderID">ID of the destination folder</param>
		/// <param name="tagList">Tags which need to be assigned to the target file</param>
		/// <param name="addToMyBoxCompleted">Delegate which will be executed after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="addToMyBoxCompleted"/> is null</exception>
		public void AddToMyBox(long targetFileID, long destinationFolderID, TagPrimitiveCollection tagList, OperationFinished<AddToMyBoxStatus> addToMyBoxCompleted)
		{
			ThrowIfParameterIsNull(addToMyBoxCompleted, "addToMyBoxCompleted");

			_service.add_to_myboxCompleted += AddToMyBoxFinished;

			_service.add_to_myboxAsync(API_KEY, _token, targetFileID, null, destinationFolderID, ConvertTagPrimitiveCollection2String(tagList), addToMyBoxCompleted);
		}

		/// <summary>
		/// Asuncronously copies a file publicly shared by someone to a user's folder
		/// </summary>
		/// <param name="targetFileName">Name of the file which needs to be copied</param>
		/// <param name="destinationFolderID">ID of the destination folder</param>
		/// <param name="tagList">Tags which need to be assigned to the target file</param>
		/// <param name="addToMyBoxCompleted">Callback method which will be invoked after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="addToMyBoxCompleted"/> is null</exception>
		public void AddToMyBox(string targetFileName, long destinationFolderID, TagPrimitiveCollection tagList, OperationFinished<AddToMyBoxStatus> addToMyBoxCompleted)
		{
			ThrowIfParameterIsNull(addToMyBoxCompleted, "addToMyBoxCompleted");

			_service.add_to_myboxCompleted += AddToMyBoxFinished;

			_service.add_to_myboxAsync(API_KEY, _token, 0, targetFileName, destinationFolderID, ConvertTagPrimitiveCollection2String(tagList), addToMyBoxCompleted);
		}

		private void AddToMyBoxFinished(object sender, add_to_myboxCompletedEventArgs e)
		{
			OperationFinished<AddToMyBoxStatus> addToMyBoxCompleted = (OperationFinished<AddToMyBoxStatus>) e.UserState;
			AddToMyBoxStatus status = StatusMessageParser.ParseAddToMyBoxStatus(e.Result);

			switch (status)
			{
				case AddToMyBoxStatus.ApplicationRestricted:
				case AddToMyBoxStatus.Failed:
				case AddToMyBoxStatus.LinkExists:
				case AddToMyBoxStatus.NotLoggedIn:
				case AddToMyBoxStatus.Successful:
					addToMyBoxCompleted(status, null);
					break;
				default:
					addToMyBoxCompleted(status, e.Result);
					break;
			}
		}

		#endregion

		#region PublicShare

		/// <summary>
		/// Publicly shares a file or folder
		/// </summary>
		/// <param name="targetObjectID">ID of the object to be shared</param>
		/// <param name="targetObjectType">Type of the object</param>
		/// <param name="password">Password to protect shared object or Null</param>
		/// <param name="notificationMessage">Message to be included in a notification email</param>
		/// <param name="emailList">Array of emails for which to notify users about a newly shared file or folder</param>
		/// <param name="publicName">Unique identifier of a publicly shared object</param>
		/// <returns>Operation status</returns>
		public PublicShareStatus PublicShare(long targetObjectID, ObjectType targetObjectType, string password, string notificationMessage, string[] emailList, out string publicName)
		{
			string type = ObjectType2String(targetObjectType);
			string result = _service.public_share(API_KEY, _token, type, targetObjectID, password, notificationMessage, emailList, out publicName);

			return StatusMessageParser.ParsePublicShareStatus(result);
		}

		/// <summary>
		/// Asynchronously publicly shares a file or folder
		/// </summary>
		/// <param name="targetObjectID">ID of the object to be shared</param>
		/// <param name="targetObjectType">Type of the object</param>
		/// <param name="password">Password to protect shared object or Null</param>
		/// <param name="message">Message to be included in a notification email</param>
		/// <param name="emailList">Array of emails for which to notify users about a newly shared file or folder</param>
		/// <param name="sendNotification">Indicates if the notification about object sharing must be send</param>
		/// <param name="publicShareCompleted">Callback method which will be invoked after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="publicShareCompleted"/> is null</exception>
		public void PublicShare(long targetObjectID, ObjectType targetObjectType, string password, string message, string[] emailList, bool sendNotification, OperationFinished<PublicShareStatus, string> publicShareCompleted)
		{
			ThrowIfParameterIsNull(publicShareCompleted, "publicShareCompleted");

			string type = ObjectType2String(targetObjectType);

			_service.public_shareCompleted += PublicShareFinished;
			_service.private_shareAsync(API_KEY, _token, type, targetObjectID, emailList, message, sendNotification, publicShareCompleted);
		}

		private void PublicShareFinished(object sender, public_shareCompletedEventArgs e)
		{
			OperationFinished<PublicShareStatus, string> publicShareCompleted = (OperationFinished<PublicShareStatus, string>)e.UserState;
			PublicShareStatus status = StatusMessageParser.ParsePublicShareStatus(e.Result);

			switch (status)
			{
				case PublicShareStatus.Successful:
				case PublicShareStatus.Failed:
				case PublicShareStatus.ApplicationRestricted:
				case PublicShareStatus.NotLoggedIn:
				case PublicShareStatus.WrongNode:
					publicShareCompleted(status, e.public_name, null);
					break;
				default:
					publicShareCompleted(status, e.public_name, e.Result);
					break;
			}
		}

		#endregion

		#region PublicUnshare

		/// <summary>
		/// Unshares a shared object
		/// </summary>
		/// <param name="targetObjectID">ID of the object to be unshared</param>
		/// <param name="targetObjectType">Type of the object</param>
		/// <returns>Operation status</returns>
		public PublicUnshareStatus PublicUnshare(long targetObjectID, ObjectType targetObjectType)
		{
			string type = ObjectType2String(targetObjectType);
			string result = _service.public_unshare(API_KEY, _token, type, targetObjectID);

			return StatusMessageParser.ParsePublicUnshareStatus(result);
		}

		/// <summary>
		/// Asynchronously unshares a shared object
		/// </summary>
		/// <param name="targetObjectID">ID of the object to be unshared</param>
		/// <param name="targetObjectType">Type of the object</param>
		/// <param name="publicUnshareCompleted">Callback method which will be invoked after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="publicUnshareCompleted"/> is null</exception>
		public void PublicUnshare(long targetObjectID, ObjectType targetObjectType, OperationFinished<PublicUnshareStatus> publicUnshareCompleted)
		{
			ThrowIfParameterIsNull(publicUnshareCompleted, "publicUnshareCompleted");

			string type = ObjectType2String(targetObjectType);
			
			_service.public_unshareCompleted += PublicUnshareCompleted;

			_service.public_unshareAsync(API_KEY, _token, type, targetObjectID, publicUnshareCompleted);
		}

		private void PublicUnshareCompleted(object sender, public_unshareCompletedEventArgs e)
		{
			OperationFinished<PublicUnshareStatus> publicUnshareCompleted = (OperationFinished<PublicUnshareStatus>) e.UserState;
			PublicUnshareStatus status = StatusMessageParser.ParsePublicUnshareStatus(e.Result);

			switch (status)
			{
				case PublicUnshareStatus.Successful:
				case PublicUnshareStatus.Failed:
				case PublicUnshareStatus.NotLoggedIn:
				case PublicUnshareStatus.WrongNode:
				case PublicUnshareStatus.ApplicationRestricted:
					publicUnshareCompleted(status, null);
					break;
				default:
					publicUnshareCompleted(status, e.Result);
					break;
			}
		}

		#endregion

		#region PrivateShare

		/// <summary>
		/// Privately shares an object with another user(s)
		/// </summary>
		/// <param name="targetObjectID">ID of the object to be shared</param>
		/// <param name="targetObjectType">Type of the object</param>
		/// <param name="password">Password to protect shared object or Null</param>
		/// <param name="notificationMessage">Message to be included in a notification email</param>
		/// <param name="emailList">Array of emails for which to notify users about a newly shared file or folder</param>
		/// <param name="sendNotification">Indicates if the notification about object sharing must be send</param>
		/// <returns>Operation status</returns>
		public PrivateShareStatus PrivateShare(long targetObjectID, ObjectType targetObjectType, string password, string notificationMessage, string[] emailList, bool sendNotification)
		{
			string type = ObjectType2String(targetObjectType);
			string result = _service.private_share(API_KEY, _token, type, targetObjectID, emailList, notificationMessage, sendNotification);

			return StatusMessageParser.ParsePrivateShareStatus(result);
		}

		/// <summary>
		/// Asynchronously privately shares an object with another user(s)
		/// </summary>
		/// <param name="targetObjectID">ID of the object to be shared</param>
		/// <param name="targetObjectType">Type of the object</param>
		/// <param name="password">Password to protect shared object or Null</param>
		/// <param name="notificationMessage">Message to be included in a notification email</param>
		/// <param name="emailList">Array of emails for which to notify users about a newly shared file or folder</param>
		/// <param name="sendNotification">Indicates if the notification about object sharing must be send</param>
		/// <param name="privateShareCompleted">Callback method which will be invoked after operation completes</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="privateShareCompleted"/> is null</exception>
		public void PrivateShare(long targetObjectID, ObjectType targetObjectType, string password, string notificationMessage, string[] emailList, bool sendNotification, OperationFinished<PrivateShareStatus> privateShareCompleted)
		{
			if(privateShareCompleted == null)
			{
				throw new ArgumentException("'privateShareCompleted' can not be null");
			}

			string type = ObjectType2String(targetObjectType);

			_service.private_shareCompleted += PrivateShareFinished;
			
			_service.private_shareAsync(API_KEY, _token, type, targetObjectID, emailList, notificationMessage, sendNotification, privateShareCompleted);
		}

		private void PrivateShareFinished(object sender, private_shareCompletedEventArgs e)
		{
			OperationFinished<PrivateShareStatus> privateShareCompleted = (OperationFinished<PrivateShareStatus>) e.UserState;
			PrivateShareStatus status = StatusMessageParser.ParsePrivateShareStatus(e.Result);

			switch (status)
			{
				case PrivateShareStatus.Successful:
				case PrivateShareStatus.Failed:
				case PrivateShareStatus.ApplicationRestricted:
				case PrivateShareStatus.NotLoggedIn:
				case PrivateShareStatus.WrongNode:
					privateShareCompleted(status, null);
					break;
				default:
					privateShareCompleted(status, e.Result);
					break;
			}
		}

		#endregion


		#region Helper method

		/// <summary>
		/// Throws ArgumentException if <paramref name="parameter"/> is null
		/// </summary>
		/// <param name="parameter">Parameter which needs to be checked</param>
		/// <param name="parameterName">Parameter name</param>
		private static void ThrowIfParameterIsNull(object parameter, string parameterName)
		{
			if (parameter == null)
			{
				throw new ArgumentException(string.Format("'{0}' can not be null", parameterName));
			}
		}


		/// <summary>
		/// Converts list of tags to comma-separated string which contains tags' IDs
		/// </summary>
		/// <param name="tagList">List of tags</param>
		/// <returns>Comma-separated string which contains tags' IDs</returns>
		private static string ConvertTagPrimitiveCollection2String(TagPrimitiveCollection tagList)
		{
			StringBuilder result = new StringBuilder();

			foreach (TagPrimitive tag in tagList)
			{
				result.Append(tag.ID + ",");
			}

			if (result.Length > 0)
			{
				result.Remove(result.Length - 1, 1);
			}

			return result.ToString();
		}

		/// <summary>
		/// Converts <paramref name="objectType"/> to string representation
		/// </summary>
		/// <param name="objectType">Object type</param>
		/// <returns>String representation of <paramref name="objectType"/> variable</returns>
		/// <exception cref="NotSupportedObjectTypeException">Thrown when method can't convert <paramref name="objectType"/> variable to String</exception>
		private static string ObjectType2String(ObjectType objectType)
		{
			string type;

			switch (objectType)
			{
				case ObjectType.File:
					type = "file";
					break;
				case ObjectType.Folder:
					type = "folder";
					break;
				default:
					throw new NotSupportedObjectTypeException(objectType);
			}

			return type;
		}

		/// <summary>
		/// Parses XML folder structure message
		/// </summary>
		/// <param name="message">Folder structure message</param>
		/// <returns>Parsed folder structure</returns>
		private Folder ParseFolderStructureXmlMessage(string message)
		{
			Expression<Func<User>> materializeUser = () => User;
			Expression<Func<long, TagPrimitive>> materializeTag = tagID => GetTag(tagID);

			return MessageParser.Instance.ParseFolderStructureMessage(message, materializeUser, materializeTag);
		}

		/// <summary>
		/// Extracts first file from zip archive
		/// </summary>
		/// <param name="input">ZIP archive content</param>
		/// <returns>Content of the first ZIPed file or empty byte array</returns>
		private static byte[] Unzip(byte[] input)
		{
			byte[] output;
			byte[] buffer = new byte[1024];

			using (MemoryStream resultStream = new MemoryStream())
			{
				using (MemoryStream inputStream = new MemoryStream())
				{
					inputStream.Write(input, 0, input.Length);
					inputStream.Flush();
					inputStream.Seek(0, SeekOrigin.Begin);
					
					ZipFile zipArchive = new ZipFile(inputStream);

					if (zipArchive.Count > 0 && zipArchive[0].IsFile && zipArchive[0].CanDecompress)
					{
						using (Stream decompressor = zipArchive.GetInputStream(0))
						{
							int readBytes;

							while ((readBytes = decompressor.Read(buffer, 0, buffer.Length)) != 0)
							{
								resultStream.Write(buffer, 0, readBytes);
							}

							decompressor.Close();
						}
					}

					zipArchive.Close();
					
					inputStream.Close();
				}

				output = new byte[resultStream.Length];

				resultStream.Flush();
				resultStream.Seek(0, SeekOrigin.Begin);
				resultStream.Read(output, 0, output.Length);

				resultStream.Close();
			}

			return output;
		}

		#endregion
	}
}
