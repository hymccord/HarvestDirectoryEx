# HarvestDirectoryEx

Table of contents
-----------------

* [Introduction](#introduction)
* [Installation](#installation)
* [Usage](#usage)
* [Known issues and limitations](#known-issues-and-limitations)
* [Getting help](#getting-help)
* [Contributing](#contributing)


Introduction
------------

HarvestDirectoryEx is a WiX heat extension that adds extended directory harvest capability than the default HarvestDirectory target.

The extension adds support for include and exclude glob patterns.

Installation
------------

At the time of writing, the support of WiX3 wixproj nuget packages is shaky at best.
* You will need to be using a packages.config for your wixproj
* Use nuget restore to restore nuget packages (not `dotnet restore` or `msbuild /restore`)
* Manually import the props and targets into your wixproj
    * At the top of wixproj
        * ```<Import Project="<path_to_packages_folder>\HarvestDirectoryEx.<VERSION>\build\HarvestDirectoryEx.props```
    * At the bottom of wixproj
        * ```<Import Project="<path_to_packages_folder>\HarvestDirectoryEx.<VERSION>\build\HarvestDirectoryEx.targets```

Usage
-----

The **HarvestDirectoryEx** target follows the same procedure as the **HarvestDirectory** target. Read the v3 documentation about that [here](https://wixtoolset.org/documentation/manual/v3/msbuild/target_reference/harvestdirectory.html).

The HarvestDirectoryEx target passes HarvestDirectoryEx items to the [HeatDirectoryEx task](./HarvestDirectoryEx/BuildTasks/HeatDirectoryEx.cs) to generate authoring from a directory that matches the include and exclude patterns.

```xml
<ItemGroup>
  <HarvestDirectoryEx Include="..\bin\Debug">
    <Include>**\*Good.dll</Include>
    <Exlcude>**\Secret\*Bad*.dll;AnotherDir\Specific.dll<Exclude>
  </HarvestDirectoryEx>
</ItemGroup>
```

### Items
| Item or MetaData | Description |
| --- | --- |
| `%(HarvestDirectoryEx.Include)` | Optional **string** metadata. <br /> Glob pattern to only author matching files. Separate mutiple patterns with semicolons. The default is `**\*.*`|
| `%(HarvestDirectoryEx.Exclude)` | Optional **string** metadata. <br /> Glob pattern to exclude files after matching include. Separate multiple patterns with semicolons. The default is **null**|

If you look at the documentation for HarvestDirectory, HarvestDirectoryEx retains all the same Items and Properties but generally the properties will use `$(HarvesDirectoryEx...)`, but they default to their respective HarvestDirectory namesake.

Known issues and limitations
----------------------------

If you are not suppressing registry entries, then all dependendent DLLs need to be found alongside the original dll. This is not a limitation of this extension but of WiX and why this extension has been built. It's useful to exclude those dlls if you know they will be found on the target system (like your project a plugin).


Getting help
------------

Contact Hank if you need help authoring WiX extensions. Or you can browse the WiX3 toolset source [here](https://github.com/wixtoolset/wix3) or the WiX4 toolset source [here](https://github.com/wixtoolset/wix4) (still in preview).


Contributing
------------

Checkout and build. GitVersion is used for versioning.

Any commits to `development` and `release` will generate an `alpha` or `beta` packages respectively.
Commits to master need to be tagged for an official release package.
