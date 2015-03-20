**!!! IMPORTANT !!!:** This project is no longer under development.

## Introduction ##

`BoxSync.NET` is a .Net Library for accessing the Box.NET API. Written entirely in C# it can be accessed from with any .Net language in .Net Framework 3.5

## Releases ##

Last release of `BoxSync.NET` is available at
> [BoxSync.NET 0.4.1.0](http://boxsyncnet.googlecode.com/files/BoxSync.NET%200.4.1.0.zip)

Documentation could be found here
> [BoxSync.Core.Doc.chm](http://boxsyncnet.googlecode.com/files/BoxSync.Core.Doc.chm)

### Short description of release 0.4.1.0: ###
  * Added "CopyFile(...)" method

### Short description of release 0.3.4.0: ###
  * Added support for "public\_name" attribute.
Now after file was uploaded as shared `File.PublicName` property contains its public name which users can use as a part of URL to view file.

### Short description of release 0.3.3.0: ###
  * Added support for "Simple" folder structure retrieval option.
_After updating your solution with new version of BoxSync.NET library you would need to add minor changes in your code._

## Getting the code ##

View the trunk at:
<p>
<blockquote><a href='http://boxsyncnet.googlecode.com/svn/trunk/'>http://boxsyncnet.googlecode.com/svn/trunk/</a>
</p></blockquote>

## Documentation ##

**How to use `BoxManager` class:**
```
IWebProxy webProxy = new WebProxy("127.0.0.1", 8080);
BoxManager manager = new BoxManager(YOUR_APPLICATION_API_KEY, "http://box.net/api/soap", webProxy);

Folder rootFolder;

GetAccountTreeStatus status = manager.GetRootFolderStructure(RetrieveFolderStructureOptions.NoZip | RetrieveFolderStructureOptions.NoFiles, out rootFolder);

if (status == GetAccountTreeStatus.Successful)
{
	// place your code here
}
```

### Samples ###
  1. [How to authenticate user](http://code.google.com/p/boxsyncnet/wiki/Authentication)
  1. [How to retrieve folder tree](http://code.google.com/p/boxsyncnet/wiki/GetFolderStructure)
  1. [How to upload files](http://code.google.com/p/boxsyncnet/wiki/FileUpload)