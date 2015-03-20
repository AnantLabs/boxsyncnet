## Authentication process ##

There are two ways to authenticate user on Box.NET.

The first and easiest way is to use "AuthenticateUser" method from **BoxManager** class. But in order to start using it you will need to contact [developer support team](mailto:developers@box.net) and request access for your application.

The second way requires to apply the following steps:
  1. Obtain a ticket by calling the "GetTicket" method
  1. Redirect the user to "www.box.net/api/1.0/auth/{TICKET}". This will display a Box.net authentication page that highlights your service
  1. The user then authenticates by entering their user name and password on the authentication page
  1. After successful user authentication call "GetAuthenticationToken" method to get authentication token which is used to access other Box.NET methods

## Sample code ##

**The following code shows how the authentication methods could be used in your application:**
```
	public class BoxProvider
	{
		private readonly BoxManager _manager;
		private string _ticket;
		
		public BoxProvider(string applicationApiKey)
		{
			_manager = new BoxManager(applicationApiKey, "http://box.net/api/soap", null);
		}

		/// <summary>
		/// Asynchronously gets authorization ticket 
		/// and opens web browser to logging on Box.NET portal
		/// </summary>
		public void StartAuthentication()
		{
			_manager.GetTicket(GetTicketCompleted);
		}

		/// <summary>
		/// Finishes authorization process after user has 
		/// successfully finished loggin process on Box.NET portal
		/// </summary>
		/// <param name="printUserInfoCallback">Callback method which will be invoked after operation completes</param>
		public void FinishAuthentication(Action<User> printUserInfoCallback)
		{
			_manager.GetAuthenticationToken(_ticket, GetAuthenticationTokenCompleted, printUserInfoCallback);
		}

		private void GetAuthenticationTokenCompleted(GetAuthenticationTokenResponse response)
		{
			Action<User> printUserInfoCallback = (Action<User>)response.UserState;

			printUserInfoCallback(response.AuthenticatedUser);
		}

		private void GetTicketCompleted(GetTicketResponse response)
		{
			if (response.Status == GetTicketStatus.Successful)
			{
				_ticket = response.Ticket;

				string url = string.Format("www.box.net/api/1.0/auth/{0}", response.Ticket);
				
				BrowserLauncher.OpenUrl(url);
			}
		}
	}
```


**You can use sample class as it's shown below:**
```
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Press ENTER to start...");

			BoxProvider boxProvider = new BoxProvider("PUT_HERE_YOUR_APPLICATION_API_KEY");

			boxProvider.StartAuthentication();

			Console.WriteLine(@"Type ""1"" and press ENTER after successful logging on Box.NET portal...");

			while(Console.ReadLine() != "1")
			{}

			boxProvider.FinishAuthentication(PrintUserInformation);

			Console.ReadLine();
		}

		private static void PrintUserInformation(User user)
		{
			Console.WriteLine(string.Format("Login: {0}", user.Login));
			Console.WriteLine(string.Format("Email: {0}", user.Email));
			Console.WriteLine(string.Format("Available space: {0}", user.SpaceAmount));
			Console.WriteLine(string.Format("Used space: {0}", user.SpaceUsed));
		}
	}
```


**To open a web page use code from http://dotnetpulse.blogspot.com/2006/04/opening-url-from-within-c-program.html :**
```
internal static class BrowserLauncher
	{
		/// <summary>
		/// Reads path of default browser from registry
		/// </summary>
		/// <returns>Path to the default web browser executable file</returns>
		private static string GetDefaultBrowserPath()
		{
			string key = @"htmlfile\shell\open\command";

			RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(key, false);

			return ((string)registryKey.GetValue(null, null)).Split('"')[1];

		}

		/// <summary>
		/// Opens <paramref name="url"/> in a default web browser
		/// </summary>
		/// <param name="url">Destination URL</param>
		public static void OpenUrl(string url)
		{
			string defaultBrowserPath = GetDefaultBrowserPath();

			Process.Start(defaultBrowserPath, url);
		}
	}
```