# Instructions #

---


### Referencing the API in an Application ###
Follow these instructions to prepare your application for a future application reinstallation from a different install point.  These steps add the necessary library references so that your application can automatically reinstall from a new location.  These steps can be followed at any point, even if a new installation is not yet necessary.

  1. Open the ClickOnceReinstaller project and build the project in Release mode.
  1. Open your ClickOnce application and a reference to the ClickOnceReinstaller.dll file to your start up project.  Alternatively, you can add the ClickOnceReinstaller project to your application and refrence the project.
  1. Next, open the code file containing the entry point for your application. (In C# this is typically Program.cs)
  1. From within the application entry point file, make a call to the Reinstaller.CheckForUpdates() function.  There are a couple of method signatures for CheckForUpdates().  See the Intellisense descriptions for determining which signature to call for your application.  Initially, this should not matter because the necessary look-up file should not be published to  your installation server.
  1. (OPTIONAL) The Reinstaller.CheckForUpdates method returns an InstallStatus object which is an enumerated value of the state of the installation process.  Capture this value and handle it accordingly.  The definitions for each potential return value can be found through Intellisense for each value. A NoUpdates response means that there are currently no new updates require a reinstallation of your application.
  1. Test compile your application and republish a new version of the application to the installation server.


### Updating an Application from a New Install Location ###
These steps are required once an application needs to move to a new web address or a change needs to be made to the application requiring the reinstallation of the application.

If your web server needs to move to a new location, it is highly recommended that you follow these steps and implement the new install point before taking the current ClickOnce install point offline.

  1. In a text editor, create a new file.
  1. On the first line of the file, add the fully qualified location for the new install location.  (i.e. Save the new file to http://www.example.com/ClickOnceInstall_NewLocation/)
  1. Save the file as "reinstall" to the root of your current applications ClickOnce install location.  (i.e http://www.example.com/ClickOnceInstall/reinstall where http://www.example.com/ClickOnceInstall/ is the root of the installation path.)
  1. Launch your application from your test machine.  The application should automatically uninstall your current version of your application and reinstall it from the location specified in the reinstall file.


### Special Notes ###
  * You do not have to save the reinstall file to the root of the original application installation folder, however, you will need to publish an version of your application to the original install point that references a web address that will contain a reinstall file that will specify the new install point.  This requires a bit of pre-planning so that a reference can be made from the application to a path that you know you will have control of.
  * The reinstall file can be saved to the root of the initial install location but must be left empty if the application does not yet need to be reinstalled.  An empty reinstall file is ignored.
  * Technically, the API looks for a web resonse from a call to "reinstall".  A mechanism could potentially be implemented on the server that returns a text reponse with the location of the new installation.
  * The reinstall file is parsed by looking at the first line of the file for the location of the new installation.  All other text is ignored.  This is intentional so that subsequent updates to this API can potentially implement newer properties in the reinstall response.
  * The API in it's current state will only support ClickOnce applications that  have been installed under an English culture variant.  The reason for this constraint is because the process is automated by looking for the uninstall dialog and passing a Click command to a button that has the text value of "OK".
  * The API can be easily modified to determine the installed cultural settings for the application and looking for the specific text variant of "OK" for the specific culture.  If you would like to help modify this project, please send a message to a deve team member.
  * If there is enough there is enough interest, I'll change the API to work with other culture specific installations.