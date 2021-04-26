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

## 1. Identification of the problem

The IFC4x1 format defines 126 different concrete classes that inherit from ```IfcRepresentationItem``` for the purpose of model representation, some of which (for example _IfcSurfaceCurveSweptAreaSolid_, _IfcRevolvedAreaSolidTapered_, _IfcBooleanClippingResult_, and many more) can only be converted for 3D rendering using computationally complex and time consuming algorithms.

This makes the IFC format unsuitable for the transmission of geometries for visualization purposes, particularly for  devices with limited computational power and storage capacity, which can not be discounted in mobile and web deployment scenarios.

Consequently, it is common practice to convert the geometric content of IFC models into simpler schemas that, albeit dropping the powerful descriptive power of IFC, require minimal or null data transformation ahead of the 3D representation process, lowering the bar for the adoption of a variety of 3D visualization components.
This conversion process is generally performed once in the controlled environment of a custom server where performance and resources can be balanced to guarantee reasonable user experiences, with the _simplified geometry_  being retained on the servers, next to the original IFC source, to prevent information loss.

While transfer of the _simplified geometry_ would be sufficient for the mere visualization of the model to any network client, it would lack any semantic property that is needed for most _use cases_<sup id="a2">[2](#f2)</sup> in the management of building assets.
While this limitation would be conceptually surpassed with the transfer of the associated _IFC source_, this has clear drawbacks. First and foremost, it would obviously result in duplicate data transfer; second, it would still require relatively complex algorithms on the client, given the complexity of the IFC format.

## 2. Aim and Objectives

The analysis of the problems, resulted in the formalisation of the following research aim: "Develop and test a sustainable workflow for the distributed representation and management of built assets across a platform-agnostic information technology stack."

### 2.1 Objectives

The proposed aim was later specialised with the definition of the following Objectives:

1. __O1__ the identification of a _geometry transfer format_ (GTF) that enabled deployment across a wide range of client environments,
2. __O2__ a strategy for efficient transfer of non-geometric data for a range of business workflows, and
3. __O3__ a software solution supporting the implementation of the identified format and strategy.

### 2.1. Approach

Furthermore, a list of guiding criteria were enumerated to support decision making in consideration of the nature of the industrial sector and the technical landscape:

1. __C1__: a preference for open standard and open source algorithms. The long lasting nature of assets in the _Built Environment_ and its stark contrast with fast obsolescence of digital solutions, determined a preference for open and accessible IT workflows as a way to mitigate the risks of technological lock-in as well as enhance the chance and reduce the cost of digital content preservation.
1. __C2__: exact content experience. When dealing with diverse software environments it is often impossible to retain the same user interface on all platform; while this is acceptable, and sometimes beneficial, it is important that the identical content remains available across them.
1. __C3__: Following current practice the research aims at a solution that can be integrated in a RESTful infrastructure that favours storage over computation on the service side.

## 3. Development cycle

### 3.1 Format selection

An initial review has been performed to select a long-list of suitable 3D formats; which has led to the following candidates: COLLADA, FBX, glTF, PLY, PRC, STL, Universal 3D, VRML (X3D), and Wavefront Obj.

- __Collada__: Format maintained by Chronos Group published ad ISO/PAS 17506, initial release 2004, last release 2008. XML format with public XSD schema definition and good coverage of loaders (also known as DAE).
- __FBX__: Format acquired and maintained by Autodesk, which regularly publishes a C++ SDK with bindings for Python; includes motion, with 2D, 3D, audio, and video data. Most popular in the gaming domain.
- __glTF__: Format maintained by Chronos Group, started in 2013 and still active. Reference implementation is available as well as ports to several languages and environments.
- __PLY__: Flexible 3D format specified by Greg Turk; excluded since it does not allow the definition of multiple objects.
- __PRC__: Highly compressed 3D file developed by Adobe Systems. Last updated 2008. Powerful ability to describe alternative representation levels. Allows embedding of tri-dimensional models within PDF files.
- __STL__: Widely popular format for triangulated meshes; excluded since it does not allow the definition of multiple objects.
- __Universal 3D__: Accepted as ECMA-363 standard in August 2005. 3D PDF documents support U3D objects embedding and can be viewed in Adobe Reader. Provides definitions for higher order primitives (curved surfaces) and custom blocks.
- __VRML (X3D)__:
- __Wavefront Obj__:


```
minimizes both the size of 3D assets, and the runtime processing needed to unpack and use those assets
```

```
it is an ecosystem of tools, documentation, and extensions contributed by the community
```


### 3.2 Model reduction

### 3.3 Flexible architecture

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
Install-Package Xbim.Gltf.IO -Source https://www.myget.org/F/xbim-master/api/v3/index.json
```

## Library authors

* **Claudio Benghi** - *Initial work* - [Personal page](https://github.com/CBenghi)

See also the list of [contributors](https://github.com/xBimTeam/XbimGltf/graphs/contributors) who participated in this project.

## License

The library is published under the terms of the CDDL 1.0, alternative license agreements can be arranged contacting the author.

## References

<b id="f1">[1]</b> Wieringa, R. J., 2014. Design Science Methodology for Information Systems and Software Engineering. Springer-Verlag, Berlin Heidelberg. ISBN 978-3-662-43838-1. URL http://www.springer.com/gb/book/9783662438381 (accessed 5.17.2018). [↩](#a1)

<b id="f2">[2]</b> Assistant Secretary for Public Affairs, 2013. Use Cases [WWW Document]. Usability.gov. URL https://www.usability.gov/how-to-and-tools/methods/use-cases.html (accessed 9.26.20).[↩](#a2)
