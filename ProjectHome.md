# Introduction #

---

ClickOnce is an excellent framework, developed by Microsoft which provides easy installation and upgrading of Windows application to end users PCs.  Although ClickOnce has made application installation and management easier, many developers have found that published installations can be broken under certain circumstances, forcing there end-users to uninstall and reinstall the application from a new location.

This has been a sever annoyance for software distributors and single developers that distribute software through ClickOnce.

The ClickOnce Application Reinstaller API, provides automated functionally to the uninstall and re-installation process.

By including this API and adding a simple server check when an application loads, you can force your application to uninstall and reinstall with little user interaction.

# How It Works #
The API will check your web server for a file named 'reinstall' at the root of the application's installation folder.  If the reinstall file contains a web address to a new installation site, the API will automatically call the uninstall process against the current version and subsequently execute the re-installation process from the new address stored in the reinstall file.

# API Contents #
The Zipped API file contains three folders of data.

### ClickOnceReinstaller ###
This folder contains the project that you will want to compile and reference or add to your application solution.

### TestClickOnceApp ###
This folder contains a simple test solution for a console application.  You can publish this application to test server and play around with the API before integrating the API into your own application.

### Publish To Website ###
In this folder is a file named 'reinstall'.