I. General Information
======================
ClickOnceReinstaller
 - Version: 1.0.001
 - Author: Richard Hartness (rhartness@gmail.com)
 - Project Site: http://code.google.com/p/clickonce-application-reinstaller-api/

Notes:
This code has been heavily borrowed from a solution provided by
RobinDotNet (sorry, I couldn't find her actual name) on her blog,
which was a further improvement of the code posted on James Harte's
blog.  (See references below)

This code contains further improvements on the original code and
wraps it in an API which you can include into your own .Net, 
ClickOnce projects.

References:
RobinDoNet's Blog Post:
- ClickOnce and Expiring Certificates
  http://robindotnet.wordpress.com/2009/03/30/clickonce-and-expiring-certificates/
  
Jim Harte's Original Blog Post:
- ClickOnce and Expiring Code Signing Certificates
  http://www.jamesharte.com/blog/?p=11


II. Purpose
===========
The purpose of this API is to provide a simplified mechanism for transitioning 
ClickOnce applications from one install location to another.  ClickOnce can 
installations can be easily broken under varing circumstances, especially when
application certificates expire or prerequisites are upgraded.

The use of this API is to provide a stream lined, simpified approach to migrating
one version of a ClickOnce application to a subsequent update that would force 
a reinstallation of the applicaiton.

III. Instructions
=================

A. Referencing this API in current applications.
------------------------------------------------
Follow these instructions to prepare your application for a future application 
reinstallation from a different install point.  These steps add the necessary library 
references so that your application can automatically reinstall from a new location.  
These steps can be followed at any point, even if a new installation is not yet
necessary.

1. Open the ClickOnceReinstaller project and build the project in Release mode.

2. Open your ClickOnce application and a reference to the 
   ClickOnceReinstaller.dll file to your start up project.
   
   Alternatively, you can add the ClickOnceReinstaller project to your application
   and refrence the project.

3. Next, open the code file containing the entry point for your application.
   (Typically, in C#, this is Program.cs)

4. From within the application entry point file, make a call to the 
   Reinstaller.CheckForUpdates() function.  There are a couple of method
   signatures for CheckForUpdates().  See the Intellisense descriptions for 
   determining which signature to call for your application.  Initially, this 
   should not matter because the necessary look-up file should not be published to 
   your installation server.

5. (OPTIONAL) The Reinstaller.CheckForUpdates method returns an InstallStatus object 
   which is an enumerated value of the state of the installation process.  Capture this
   value and handle it accordingly.  The definitions for each potential return value
   can be found through Intellisense for each value.

   A NoUpdates response means that there are currently no new updates require a 
   reinstallation of your application.

6. Test compile your application and republish a new version of the application to the 
   installation server.


B. Updating your application from a new Install location
--------------------------------------------------------
These steps are required once an application needs to move to a new web address or
a change needs to be made to the application requiring the reinstallation of the 
application.

If your web server needs to move to a new location, it is highly recommended that you
follow these steps and implement the new install point before taking the current 
ClickOnce install point offline.

1. In a text editor, create a new file.
2. On the first line of the file, add the fully qualified location for the new install
   location.  (i.e. Save the new file to http://www.example.com/ClickOnceInstall_NewLocation/)
3. Save the file as "reinstall" to the root of your current applications ClickOnce 
   install location.  (i.e http://www.example.com/ClickOnceInstall/reinstall where 
   http://www.example.com/ClickOnceInstall/ is the root of the installation path.)
4. Launch your application from your test machine.  The application should 
   automatically uninstall your current version of your application and reinstall
   it from the location specified in the reinstall file.


C. Special Notes
----------------
1. You do not have to save the reinstall file to the root of the original 
   application installation folder, however, you will need to publish an version of
   your application to the original install point that references a web address
   that will contain a reinstall file that will specify the new install point.

   This requires a bit of pre-planning so that a reference can be made from the
   application to a path that you know you will have control of.

2. The reinstall file can be saved to the root of the initial install location
   but must be left empty if the application does not yet need to be reinstalled.  
   An empty reinstall file is ignored.

3. Technically, the API looks for a web resonse from a call to "reinstall".
   A mechanism could potentially be implemented on the server that returns a
   text reponse with the location of the new installation.

4. The reinstall file is parsed by looking at the first line of the file for
   the location of the new installation.  All other text is ignored.  This
   is intentional so that subsequent updates to this API can potentially implement
   newer properties in the reinstall response.

5. The API in it's current state will only support ClickOnce applications that 
   have been installed under an English culture variant.  The reason for this
   constraint is because the process is automated by looking for the uninstall 
   dialog and passing a Click command to a button that has the text value of 
   "OK".

   The API can be easily modified to determine the installed cultural settings
   for the application and looking for the specific text variant of "OK" for the 
   specific culture.  If you would like to help modify this project, please email
   (mailto:rhartness@gmail.com) me which culture variant (or variants) you have 
   used with your application, as well as the displayed text value on the 
   uninstallation dialog.

   If there is enough there is enough interest, I'll change the API to work with 
   other culture specific installations.


IV. Change Log
==============
v.1.0.0 - Mar-28-2011
  - Initial public release.