# XbimGltf

## Xbim.GLTF.IO

This library supports to export of coherent geometry and data from selected parts of an IFC file for consumption in other environments.

Geometry is exported in gltf format and the relative data in json. References exist between the two export sets in order restore their associations in any consuming application.

## XbimPlugin.Gltf.IO

This project provides a UI to consume the function of Xbim.GLTF.IO within the XbimXplorer application, via the plugin system.

# Usage

For most scenarios you will want to use nuget to install the library. You can do this by issuing the following command in the nuget command line

```
Install-Package Xbim.Gltf.IO -Version 4.0.1-V0007 -Source https://www.myget.org/F/xbim-develop/api/v3/index.json
```
