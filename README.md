# XbimGltf - Workflows and tools for the distributed representation of building asset via open standards

_Claudio Benghi, Northumbria university at Newcastle, UK_

26<sup>th</sup> September, 2020

__Abstract__

When working with _Building Information Models_ across diverse platforms there can be an advantage in
the storage of the model geometry in a format that is well supported across a variety of 3D viewers.

The glTF format has been identified as a candidate for such portable format, however it lacks the ability to retain
meaningful semantic properties of the geometries exported that are necessary for most business workflows in the
built environment; consequently a second format is developed to enable such workflows.

The presented solution supports production workflows under these scenarios; it allows the conversion of building geometries from the IFC format to glTF assets, establishes methods for identity management of such elements, and provide an extendable approach for the reconciliation of any semantic property lost in the conversion process.

__[pre-print working copy]__

## Methodology

The development of this library has followed a fast-tracked version of the _Design Science Methodology_ described by Wieringa <sup id="a1">[1](#f1)</sup>, which promotes the equilibrium between technical and theoretical concerns
that affect research and development projects in the Information Technology (IT) domain.

In accordance, the project was executed along the following stages:
1. identification of the problem and motivation,
2. definition of the objectives of the solution,
3. execution of the design and development cycle, and finally a
4. demonstration and evaluation stage.

## Identification of the problem

The IFC4x1 format defines 126 different concrete classes that inherit from ```IfcRepresentationItem``` for the purpose of model representation, some of which (for example _IfcSurfaceCurveSweptAreaSolid_, _IfcRevolvedAreaSolidTapered_, _IfcBooleanClippingResult_, and many more) can only be converted for 3D rendering using computationally complex and time consuming algorithms.

This makes the IFC format unsuitable for the transmission of geometries for visualization purposes, particularly for  devices with limited computational power and storage capacity, which can not be discounted in mobile and web deployment scenarios.

Consequently, it is common practice to convert the geometric content of IFC models into simpler schemas that, albeit dropping the powerful descriptive power of IFC, require minimal or null data transformation ahead of the 3D representation process, lowering the bar for the adoption of a variety of 3D visualization components.
This conversion process is generally performed once in the controlled environment of a custom server where performance and resources can be balanced to guarantee reasonable user experiences, with the _simplified geometry_  being retained on the servers, next to the original IFC source, to prevent information loss.

While transfer of the _simplified geometry_ would be sufficient for the mere visualization of the model to any network client, it would lack any semantic property that is needed for most _use cases_<sup id="a2">[2](#f2)</sup> in the management of building assets.
While this limitation would be conceptually surpassed with the transfer of the associated _IFC source_, this has clear drawbacks. First and foremost, it would obviously result in duplicate data transfer; second, it would still require relatively complex algorithms on the client, given the complexity of the IFC format.

Finally, in consideration of the long lasting nature of assets in the _Built Environment_ and the fast obsolescence of digital solutions, any developed solution required a strategy for digital preservation.

## Objectives

The analysis of the problems, resulted in the formalisation of the following research deliverables:

1. The identification of a _geometry transfer format_ (GTF) that enabled versatile deployment of business workflows
2. A strategy for efficient transfer of non-geometric data for the required workflows
3. The development of a software solution to enable the previous objectives.

### Approach

The identification of the business environment for the proposed solution also resulted in the definition of preferences that had to be considered in the development cycle.

{>> TODO: File size <<}

Strategically, a clear preference for open standard and open source algorithms was expressed as a way to maximise the chance of business continuity and continued accessibility and usability of the proposed solutions.

## Development cycle

{>> TODO: format selection <<}

{>> TODO: model reduction <<}

{>> TODO: Flexible architectural <<}

## Evaluation and Demonstration

{>> TODO: present evaluation criteria <<}

### Xbim.GLTF.IO

This library supports the export of coherent geometry and data from selected parts of an IFC file for consumption in other environments.

Geometry is exported in glTF format and the relative data in json. References exist between the two export sets in order restore their associations in any consuming workflow.

### XbimPlugin.Gltf.IO

This project provides a UI to consume the function of Xbim.GLTF.IO within the XbimXplorer application, via the plugin system.

## Usage

For most scenarios you will want to use ```nuget``` to reference the library in your project. You can do this by issuing the following command in the ```package manager command line```:

```
Install-Package Xbim.Gltf.IO -Version 4.0.1-V0007 -Source https://www.myget.org/F/xbim-develop/api/v3/index.json
```

## Library authors

* **Claudio Benghi** - *Initial work* - [PurpleBooth](https://github.com/CBenghi)

See also the list of [contributors](https://github.com/xBimTeam/XbimGltf/graphs/contributors) who participated in this project.

## License

The library is published under the terms of the CDDL 1.0, alternative license agreements can be arranged contacting the author.

## References

<b id="f1">[1]</b> Wieringa, R. J., 2014. Design Science Methodology for Information Systems and Software Engineering. Springer-Verlag, Berlin Heidelberg. ISBN 978-3-662-43838-1. URL http://www.springer.com/gb/book/9783662438381 (accessed 5.17.2018). [↩](#a1)

<b id="f1">[1]</b> Assistant Secretary for Public Affairs, 2013. Use Cases [WWW Document]. Usability.gov. URL https://www.usability.gov/how-to-and-tools/methods/use-cases.html (accessed 9.26.20).[↩](#a2)
