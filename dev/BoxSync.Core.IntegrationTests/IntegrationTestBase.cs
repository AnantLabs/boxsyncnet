using System;
using System.Configuration;
using System.Net;
using System.Threading;
using NUnit.Framework;


namespace BoxSync.Core.IntegrationTests
{
	/// <summary>
	/// Base class for all integration tests
	/// </summary>
	[TestFixture]
	public abstract class IntegrationTestBase
	{
		/// <summary>
		/// Service login
		/// </summary>
		protected string Login
		{
			get
			{
				return ConfigurationManager.AppSettings["login"];
			}
		}

		/// <summary>
		/// Service password
		/// </summary>
		protected string Password
		{
			get
			{
				return ConfigurationManager.AppSettings["password"];
			}
		}

		/// <summary>
		/// Application key
		/// </summary>
		protected string ApplicationKey
		{
			get
			{
				return ConfigurationManager.AppSettings["applicationKey"];
			}
		}

		/// <summary>
		/// Service Url
		/// </summary>
		protected string ServiceUrl
		{
			get
			{
				return ConfigurationManager.AppSettings["serviceUrl"];
			}
		}

		protected string SubmitAuthenticationInformation(string ticket)
		{
			string uploadResult = null;

			using (WebClient client = new WebClient ())
			{

				client.Headers.Add("Content-Type:application/x-www-form-urlencoded");

				Uri destinationAddress = new Uri("http://www.box.net/api/1.0/auth/" + ticket);

				ManualResetEvent submitFinishedEvent = new ManualResetEvent(false);

				Action submitLoginPassword = () =>
				{
					uploadResult = client.UploadString(destinationAddress, "POST",
													   "login=" + Login +
													   "&password=" +
													   Password +
													   "&dologin=1&__login=1");
				};

				AsyncCallback callback = asyncResult =>
				{
					ManualResetEvent submitFinished = (ManualResetEvent)asyncResult.AsyncState;

					submitFinished.Set();
				};

				IAsyncResult asyncResult2 = submitLoginPassword.BeginInvoke(callback, submitFinishedEvent);

				submitFinishedEvent.WaitOne();

				submitLoginPassword.EndInvoke(asyncResult2);
			}

			return uploadResult;
		}
	}
}
