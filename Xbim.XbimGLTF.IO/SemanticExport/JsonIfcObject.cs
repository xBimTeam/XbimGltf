using System.Collections.Generic;

namespace Xbim.GLTF.SemanticExport
{
    public class JsonIfcObject
    {
        public string ifcExpressTypeName = "undefined"; // point 1
        public string ifcEntityGuid = ""; // : (would it be even possible to have this GUID invariant between model versions => NOT GUARANTEED BY THE MOMENT),
        public int ifcEntityLabel; //:" (integer, acts like a PK on the IFC model),

        public List<IfcInfo> ifcInfo = new List<IfcInfo>(); // human readable data
    }
}