/* INSTRUCTIONS
 * Setting up this test application
 * ================================
 * 1. Create a new ClickOnce install directoryon a test web server and publish this 
 * application to the directory. 
 * (i.e. http://localhost/ClicOnceTest")
 * 
 * 2 Install the application from the ClickOnce install point.  When the application runs
 * it should return a "NoUpdates" response.
 * 
 * Migrate to a new installation
 * =============================
 * 1. Assuming the application has been published, we will need to republish the application
 * to a new web directory.  First, setup the new ClickOnce location on your web server.  
 * (i.e. http://localhost/ClicOnceTest_NewInstallPoint")
 *
 * 2. From within the ClickOnce project, you will need to increment the publishing version 
 * and republish your application to the new ClickOnce install point. NOTE: you should
 * not delete or remove the old previous install point.
 * 
 * 3. From the original install location ("http://localhost/ClicOnceTest") add a file named
 * "reinstall" (no file extension).  Edit the file with a text editor and on the first line
 * of the file add a fully qualified web address to the new install point.
 * (i.e. "http://localhost/ClicOnceTest_NewInstallPoint/ClickOnce Test App.application")  Do not 
 * include double or single quotes around the install location.  An example of this file is
 * included in the "Publish to Website" directory of this zipped archive.
 * 
 * 4. Re-run the test application.  The application should prompt that a new version is 
 * available.  After closing the message dialog, the process will automatically begin.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Deployment.Application;
using ClickOnceReinstaller;

namespace TestClickOnceApp
{
	class Program
	{
		static void Main(string[] args)
		{
			InstallStatus status = Reinstaller.CheckForUpdates(false);
			if (ApplicationDeployment.IsNetworkDeployed)
				Console.WriteLine("\t" + ApplicationDeployment.CurrentDeployment.CurrentVersion);

			if (status == InstallStatus.NoUpdates)
				Console.WriteLine("We've successfully executed the check for an update, but there was no update.");
			else if (status == InstallStatus.FailedUninstall)
				Console.WriteLine("Couldn't Uninstall the application.  New installation never executed.");
			else if (status == InstallStatus.FailedReinstall)
				Console.WriteLine("The application was uninstalled. However, the installation of the new version failed.");
			else if (status == InstallStatus.Success)
				Console.WriteLine("Success! You have uninstalled and reinstalled the application.  Here is where you can put " 
					+ "any clean up code because you didn't ask the Reinstaller to exit the application.");

			Console.ReadKey();
		}
	}
}
