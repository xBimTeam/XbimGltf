using System.Collections.Generic;
using Xbim.Common.Geometry;

namespace Xbim.GLTF.SemanticExport
{
    public class BuildingEntity : JsonIfcObject
    {
        public int ifcSystemIndex; // ": index in systems array(reference to a systems dictionary), // point 2
        public int ifcStoreyIndex; // ": index in the storeys array // for point 3, see below
        public int ifcSpaceIndex; // ": index in the spaces array // for point 4, see below

        public string gltfRepresentationGUID; // ": "323ss-as32342-sdasa2332-aasa",
        public XbimMatrix3D gltfRepresentationPlacementMatrix; //": 4x4 matrix,
    }
}