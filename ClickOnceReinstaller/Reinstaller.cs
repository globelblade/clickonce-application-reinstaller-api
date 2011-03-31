/* ClickOnceReinstaller v 1.0.0
 *  - Author: Richard Hartness (rhartness@gmail.com)
 *  - Project Site: http://code.google.com/p/clickonce-application-reinstaller-api/
 * 
 * Notes:
 * This code has heavily borrowed from a solution provided on a post by
 * RobinDotNet (sorry, I couldn't find her actual name) on her blog,
 * which was a further improvement of the code posted on James Harte's
 * blog.  (See references below)
 * 
 * This code contains further improvements on the original code and
 * wraps it in an API which you can include into your own .Net, 
 * ClickOnce projects.
 * 
 * See the ReadMe.txt file for instructions on how to use this API.
 * 
 * References:
 * RobinDoNet's Blog Post:
 * - ClickOnce and Expiring Certificates
 *   http://robindotnet.wordpress.com/2009/03/30/clickonce-and-expiring-certificates/
 *   
 * Jim Harte's Original Blog Post:
 * - ClickOnce and Expiring Code Signing Certificates
 *   http://www.jamesharte.com/blog/?p=11
 */


using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows.Forms;
using System.Xml;

namespace ClickOnceReinstaller
{
	#region Enums
	/// <summary>
	/// Status result of a CheckForUpdates API call.
	/// </summary>
	public enum InstallStatus { 
		/// <summary>
		/// There were no updates on the server or this is not a ClickOnce application.
		/// </summary>
		NoUpdates, 
		/// <summary>
		/// The installation process was successfully executed.
		/// </summary>
		Success, 
		/// <summary>
		/// In uninstall process failed.
		/// </summary>
		FailedUninstall, 
		/// <summary>
		/// The uninstall process succeeded, however the reinstall process failed.
		/// </summary>
		FailedReinstall };
	#endregion

	public static class Reinstaller
	{
		#region Public Methods

		/// <summary>
		/// Check for reinstallation instructions on the server and intiate reinstallation.  Will look for a "reinstall" response at the root of the ClickOnce application update address.
		/// </summary>
		/// <param name="exitAppOnSuccess">If true, when the function is finished, it will execute Environment.Exit(0).</param>
		/// <returns>Value indicating the uninstall and reinstall operations successfully executed.</returns>
		public static InstallStatus CheckForUpdates(bool exitAppOnSuccess)
		{
			//Double-check that this is a ClickOnce application.  If not, simply return and keep running the application.
			if (!ApplicationDeployment.IsNetworkDeployed) return InstallStatus.NoUpdates;

			string reinstallServerFile = ApplicationDeployment.CurrentDeployment.UpdateLocation.ToString();

			try
			{
				reinstallServerFile = reinstallServerFile.Substring(0, reinstallServerFile.LastIndexOf("/") + 1);
				reinstallServerFile = reinstallServerFile + "reinstall";
#if DEBUG
				Trace.WriteLine(reinstallServerFile);
				
#endif			
			} 
			catch 
			{
				return InstallStatus.FailedUninstall;
			}
			return CheckForUpdates(exitAppOnSuccess, reinstallServerFile);
		}

		/// <summary>
		/// Check for reinstallation instructions on the server and intiate reinstall.
		/// </summary>
		/// <param name="exitAppOnSuccess">If true, when the function is finished, it will execute Environment.Exit(0).</param>
		/// <param name="reinstallServerFile">Specify server address for reinstallation instructions.</param>
		/// <returns>InstallStatus state of reinstallation process.</returns>
		public static InstallStatus CheckForUpdates(bool exitAppOnSuccess, string reinstallServerFile)
		{
			string newAddr = "";

			if (!ApplicationDeployment.IsNetworkDeployed) return InstallStatus.NoUpdates;

			//Check to see if there is a new installation.
			try
			{
				HttpWebRequest rqHead = (HttpWebRequest)HttpWebRequest.Create(reinstallServerFile);
				rqHead.Method = "HEAD";
				rqHead.Credentials = CredentialCache.DefaultCredentials;
				HttpWebResponse rsHead = (HttpWebResponse)rqHead.GetResponse();

#if DEBUG
				Trace.WriteLine(rsHead.Headers.ToString());
#endif
				if (rsHead.StatusCode != HttpStatusCode.OK) return InstallStatus.NoUpdates;

				//Download the file and extract the new installation location
				HttpWebRequest rq = (HttpWebRequest)HttpWebRequest.Create(reinstallServerFile);
				WebResponse rs = rq.GetResponse();
				Stream stream = rs.GetResponseStream();
				StreamReader sr = new StreamReader(stream);
				
				//Instead of reading to the end of the file, split on new lines.
				//Currently there should be only one line but future options may be added.  
				//Taking the first line should maintain a bit of backwards compatibility.
				newAddr = sr.ReadToEnd()
					.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)[0];

				//No address, return as if there are no updates.
				if (newAddr == "") return InstallStatus.NoUpdates;
			}
			catch
			{
				//If we receive an error at this point in checking, we can assume that there are no updates.
				return InstallStatus.NoUpdates;
			}


			//Begin Uninstallation Process
			MessageBox.Show("There is a new version available for this application.  Please click OK to start the reinstallation process.");

			try
			{
				string publicKeyToken = GetPublicKeyToken();
#if DEBUG
				Trace.WriteLine(publicKeyToken);
#endif

				// Find Uninstall string in registry    
				string DisplayName = null;
				string uninstallString = GetUninstallString(publicKeyToken, out DisplayName);
				if (uninstallString == null || uninstallString == "") 
					throw new Exception("No uninstallation string was found.");
				string runDLL32 = uninstallString.Substring(0, uninstallString.IndexOf(" "));
				string args = uninstallString.Substring(uninstallString.IndexOf(" ") + 1);

#if DEBUG
				Trace.WriteLine("Run DLL App: " + runDLL32);
				Trace.WriteLine("Run DLL Args: " + args);
#endif
				Process uninstallProcess = Process.Start(runDLL32, args);
				PushUninstallOKButton(DisplayName);
			}
			catch
			{
				return InstallStatus.FailedUninstall;
			}

			//Start the re-installation process
#if DEBUG
			Trace.WriteLine(reinstallServerFile);
#endif

			try
			{
#if DEBUG
				Trace.WriteLine(newAddr);
#endif
				//Start with IE-- other browser will certainly fail.
				Process.Start("iexplore.exe", newAddr);				
			}
			catch
			{
				return InstallStatus.FailedReinstall;
			}

			if (exitAppOnSuccess) Environment.Exit(0);
			return InstallStatus.Success;
		}
		#endregion

		#region Helper Methods
		//Private Methods
		private static string GetPublicKeyToken()
		{
			ApplicationSecurityInfo asi = new ApplicationSecurityInfo(AppDomain.CurrentDomain.ActivationContext);
			
			byte[] pk = asi.ApplicationId.PublicKeyToken;
			StringBuilder pkt = new StringBuilder();
			for (int i = 0; i < pk.GetLength(0); i++)
				pkt.Append(String.Format("{0:x2}", pk[i]));

			return pkt.ToString();
		}
		private static string GetUninstallString(string PublicKeyToken, out string DisplayName)
		{
			string uninstallString = null;
			string searchString = "PublicKeyToken=" + PublicKeyToken;
#if DEBUG
			Trace.WriteLine(searchString);
#endif
			RegistryKey uninstallKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
			string[] appKeyNames = uninstallKey.GetSubKeyNames();
			DisplayName = null;
			foreach (string appKeyName in appKeyNames)
			{
				RegistryKey appKey = uninstallKey.OpenSubKey(appKeyName);
				string temp = (string)appKey.GetValue("UninstallString");
				DisplayName = (string)appKey.GetValue("DisplayName");
				appKey.Close();
				if (temp.Contains(searchString))
				{
					uninstallString = temp;
					DisplayName = (string)appKey.GetValue("DisplayName");
					break;
				}
			}
			uninstallKey.Close();
			return uninstallString;
		}
		#endregion

		#region Win32 Interop Code
		//Structs
		[StructLayout(LayoutKind.Sequential)]
		private struct FLASHWINFO
		{
			public uint cbSize;
			public IntPtr hwnd;
			public uint dwFlags;
			public uint uCount;
			public uint dwTimeout;
		}

		//Interop Declarations
		[DllImport("user32.Dll")]
		private static extern int EnumWindows(EnumWindowsCallbackDelegate callback, IntPtr lParam);
		[DllImport("User32.Dll")]
		private static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);
		[DllImport("User32.Dll")]
		private static extern void GetClassName(int h, StringBuilder s, int nMaxCount);
		[DllImport("User32.Dll")]
		private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsCallbackDelegate lpEnumFunc, IntPtr lParam);
		[DllImport("User32.Dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll")]
		private static extern short FlashWindowEx(ref FLASHWINFO pwfi);
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		//Constants
		private const int BM_CLICK = 0x00F5;
		private const uint FLASHW_ALL = 3;
		private const uint FLASHW_CAPTION = 1;
		private const uint FLASHW_STOP = 0;
		private const uint FLASHW_TIMER = 4;
		private const uint FLASHW_TIMERNOFG = 12;
		private const uint FLASHW_TRAY = 2;
		private const int FIND_DLG_SLEEP = 200; //Milliseconds to sleep between checks for installation dialogs.
		private const int FIND_DLG_LOOP_CNT = 50; //Total loops to look for an install dialog. Defaulting 200ms sleap time, 50 = 10 seconds.

		//Delegates
		private delegate bool EnumWindowsCallbackDelegate(IntPtr hwnd, IntPtr lParam);
        
		//Methods
		private static IntPtr SearchForTopLevelWindow(string WindowTitle)
		{
			ArrayList windowHandles = new ArrayList();
			/* Create a GCHandle for the ArrayList */
			GCHandle gch = GCHandle.Alloc(windowHandles);
			try
			{
				EnumWindows(new EnumWindowsCallbackDelegate(EnumProc), (IntPtr)gch);
				/* the windowHandles array list contains all of the
					window handles that were passed to EnumProc.  */
			}
			finally
			{
				/* Free the handle */
				gch.Free();
			}

			/* Iterate through the list and get the handle thats the best match */
			foreach (IntPtr handle in windowHandles)
			{
				StringBuilder sb = new StringBuilder(1024);
				GetWindowText((int)handle, sb, sb.Capacity);
				if (sb.Length > 0)
				{
					if (sb.ToString().StartsWith(WindowTitle))
					{
						return handle;
					}
				}
			}

			return IntPtr.Zero;
		}
		private static IntPtr SearchForChildWindow(IntPtr ParentHandle, string Caption)
		{
			ArrayList windowHandles = new ArrayList();
			/* Create a GCHandle for the ArrayList */
			GCHandle gch = GCHandle.Alloc(windowHandles);
			try
			{
				EnumChildWindows(ParentHandle, new EnumWindowsCallbackDelegate(EnumProc), (IntPtr)gch);
				/* the windowHandles array list contains all of the
					window handles that were passed to EnumProc.  */
			}
			finally
			{
				/* Free the handle */
				gch.Free();
			}

			/* Iterate through the list and get the handle thats the best match */
			foreach (IntPtr handle in windowHandles)
			{
				StringBuilder sb = new StringBuilder(1024);
				GetWindowText((int)handle, sb, sb.Capacity);
				if (sb.Length > 0)
				{
					if (sb.ToString().StartsWith(Caption))
					{
						return handle;
					}
				}
			}

			return IntPtr.Zero;

		}
		private static bool EnumProc(IntPtr hWnd, IntPtr lParam)
		{
			/* get a reference to the ArrayList */
			GCHandle gch = (GCHandle)lParam;
			ArrayList list = (ArrayList)(gch.Target);
			/* and add this window handle */
			list.Add(hWnd);
			return true;
		}
		private static void DoButtonClick(IntPtr ButtonHandle)
		{
			SendMessage(ButtonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
		}
		private static IntPtr FindDialog(string dialogName)
		{
			IntPtr hWnd = IntPtr.Zero;

			int cnt = 0;
			while (hWnd == IntPtr.Zero && cnt++ != FIND_DLG_LOOP_CNT)
			{
				hWnd = SearchForTopLevelWindow(dialogName);
				System.Threading.Thread.Sleep(FIND_DLG_SLEEP);
			}

			if (hWnd == IntPtr.Zero) 
				throw new Exception(string.Format("Installation Dialog \"{0}\" not found.", dialogName));
			return hWnd;
		}
		private static IntPtr FindDialogButton(IntPtr hWnd, string buttonText)
		{
			IntPtr button = IntPtr.Zero;
			int cnt = 0;
			while (button == IntPtr.Zero && cnt++ != FIND_DLG_LOOP_CNT)
			{
				button = SearchForChildWindow(hWnd, buttonText);
				System.Threading.Thread.Sleep(FIND_DLG_SLEEP);
			}
			return button;
		}
		private static bool FlashWindowAPI(IntPtr handleToWindow)
		{
			FLASHWINFO flashwinfo1 = new FLASHWINFO();
			flashwinfo1.cbSize = (uint)Marshal.SizeOf(flashwinfo1);
			flashwinfo1.hwnd = handleToWindow;
			flashwinfo1.dwFlags = 15;
			flashwinfo1.uCount = uint.MaxValue;
			flashwinfo1.dwTimeout = 0;
			return (FlashWindowEx(ref flashwinfo1) == 0);
		}
		
		//These are the only functions that should be called above.
		private static void PushUninstallOKButton(string DisplayName)
		{
			IntPtr diag = FindDialog(DisplayName + " Maintenance");
			IntPtr button = FindDialogButton(diag, "&OK");
			DoButtonClick(button);
		}
		#endregion
	}
}