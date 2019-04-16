using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xbim.GLTF.SemanticExport
{
    public class ElementPropertySet
    {
        public string propertySetName;
        public List<IfcInfo> properties = new List<IfcInfo>();
    }
}
