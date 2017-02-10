# Sample CPM Project
This sample project provides code snipets on how to call the REST api of the CPM service programmatically. 

## Prerequisites 
### Client Id and Client Secrets
Please refer to http://aka.ms/cpmapi for instructions on how to retrieve the necessary settings. This project assumes that you already have at least one topic id registered within the CPM system.

### NuGet packages
This sample project utilises a package called `Microsoft.Cpm.Api.Contracts` that is supplied and maintained by our team. We consider versions with a suffix of `-rtm` to be stable releases, however NuGet recognises them as prerelease versions.

#### Visual Studio instructions
Please configure your NuGet package manager with the following feed details:
* Name: MCKP.CPM
* Source: https://microsoft.pkgs.visualstudio.com/_packaging/MCKP.CPM/nuget/v3/index.json

When searching for the package, please ensure that the "Include Prerelease" option is **selected**. Install the latest `*-rtm` version available.

#### NuGet command line instructions
```
Install-Package Microsoft.Cpm.Api.Contracts -Source "https://microsoft.pkgs.visualstudio.com/_packaging/MCKP.CPM/nuget/v3/index.json" -Version 2.0.0-rtm-20170210 -Prerelease
```