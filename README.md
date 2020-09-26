# XbimGltf - Workflows and tools for the distributed representation of building asset via open standards

_Claudio Benghi, Northumbria university at Newcastle, 2020_

_Abstract_

When working with _Building Information Models_ across diverse platforms there can be an advantage in
the storage of the model geometry in a format that is well supported across a variety of 3D viewers.
The glTF format has been identified as a candidate for such portable format, however it lacks the ability to retain
meaningful semantic properties of the geometries exported that are necessary for most business workflows in the
built environment.

This library is designed and released to support production workflows under those scenarios; it allows the conversion of building geometries from the IFC format to glTF assets, establishes methods for identity management of such elements, and provide an extendable approach for the reconciliation of any semantic property lost in the conversion process.

## Methodology

The development of this library has followed a fast-tracked version of the _Design Science Methodology_ described by Wieringa <sup id="a1">[1](#f1)</sup>, which promotes the equilibrium between technical and theoretical concerns
that affect research and development projects in the Information Technology (IT) domain.

In accordance, the project was executed along the following stages:
- identification of the problem and motivation,
- definition of the objectives of the solution,
- execution of the design and development cycle, and finally a
- demonstration and evaluation stage.

## Identification of the problem

The IFC4x1 format defines 126 different concrete classes that inherit from ```IfcRepresentationItem``` for the purpose of model representation storage, some of which (e.g. ```IfcSurfaceCurveSweptAreaSolid```, ```IfcRevolvedAreaSolidTapered```, ```IfcBooleanClippingResult``` and many more) can only be converted to data formats suitable for 3D rendering on display adapters using computationally complex and time consuming algorithms.

This makes the IFC format unsuitable the transmission of geometries to final users for representation purposes, particularly when such algorithms need to be executed on devices with limited computational power and storage, such as mobile devices or web environments.

Consequently, it is common practice to convert the geometric content of IFC models to simpler schemas that, albeit dropping the powerful descriptive power of IFC, enable fast and simple 3D representation of the elements in a variety of user interfaces. This conversion process is generally performed once and retained next to the original IFC source for Information completeness.

definition of workflows and tools for the management of building asset through open standards.  
definition of workflows and tools for the management of building asset through open standards.  

## Objectives

To be completed

## Development cycle

To be completed

## Demonstration

To be completed

### Xbim.GLTF.IO

This library supports to export of coherent geometry and data from selected parts of an IFC file for consumption in other environments.

Geometry is exported in gltf format and the relative data in json. References exist between the two export sets in order restore their associations in any consuming application.

### XbimPlugin.Gltf.IO

This project provides a UI to consume the function of Xbim.GLTF.IO within the XbimXplorer application, via the plugin system.

## Usage

For most scenarios you will want to use nuget to install the library. You can do this by issuing the following command in the nuget command line

```
Install-Package Xbim.Gltf.IO -Version 4.0.1-V0007 -Source https://www.myget.org/F/xbim-develop/api/v3/index.json
```

## Author

This library is developed by Claudio Benghi @CBenghi

## License

The library is published under the terms of the CDDL 1.1, alternative license agreements can be arranged contacting the author.

## References

<b id="f1">[1]</b> Wieringa, R. J., 2014. Design Science Methodology for Information Systems and Software Engineering. Springer-Verlag, Berlin Heidelberg. ISBN 978-3-662-43838-1. URL http://www.springer.com/gb/book/9783662438381 (accessed 5.17.2018). [↩](#a1)
