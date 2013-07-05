Visual Studio Extension
=======================

Visual Studio 2012 Extension for integrating Umbraco with a web solution.

## Building the solution ##

Before opening and building the solution you will need to the have [Visual Studio 2012 SDK](http://www.microsoft.com/en-us/download/details.aspx?id=30668) installed.

The solution consists of three projects:

- Umbraco.VisualStudio - this is acts as the bridge between Visual Studio and the Umbraco API.
- UmbracoStudio - this is the actual Visual Studio extension and where the magic happens.
- UmbracoStudioApp - this is a replica of the Visual Studio extension as a WPF app (which is currently not up-to-date).

Once you build the solution a UmbracoStudio.vsix file (Visual Studio package) will be created in the bin/Debug folder, which you can use to install the extension.

## Using the Extension ##

This extension is only usable with version 6.1.2 (or newer) of Umbraco.

In order to use the extension you should have Umbraco configured as an ASP.NET Webforms Web Application or an ASP.NET MVC Web Application, and the database also has to be installed before using it.

The extension will work with both Sql Server and Sql Server CE Databases.

Once you have Umbraco up and running in Visual Studio (The easiest way would be to use the UmbracoCms nuget package), you go to the View menu, Other Windows and click Umbraco Studio. An explorer window will pop up, which you can dock like any other window.

If no tree is shown there was a problem loading the Umbraco solution. You can click the Umbraco logo to check everything looks correct.

If everything is working as expected you should see folders for Content, ContentTypes, Media, MediaTypes and DataTypes.
You can navigate the tree and do a limited set of actions like Delete, Rename, Move to Trash and Serialize.


## Current Release: CTP ##

There is currently a Community Technical Preview available of this extension, which you can download from the [release page](https://github.com/umbraco/Visual-Studio-Extension/releases).

**As of now only a CTP release is available and please understand that it is still in a very early stage and therefor also unstable. But please try it out and provide feedback.**

Any questions please ask [@sitereactor](http://twitter.com/sitereactor) on twitter or post an [issue](https://github.com/umbraco/Visual-Studio-Extension/issues).
