using System.Collections.Generic;
using Xbim.Ifc4.Interfaces;

namespace Xbim.GLTF.SemanticExport
{
    public class JsonIfcObject
    {
        /// <summary>
        /// This should serve point 1 of the specs
        /// </summary>
        public string ifcExpressTypeName = "undefined"; 

        /// <summary>
        /// Applications are responsible to keep GUIDs constan across Ifc export, but this is not always the case
        /// </summary>
        public string ifcGuid = ""; 

        /// <summary>
        /// integer, acts like a PK on the IFC model
        /// </summary>
        public int ifcEntityLabel;

        /// <summary>
        /// human readable data
        /// </summary>
        public List<IfcInfo> ifcInfo = new List<IfcInfo>();

        virtual internal void SetBase(IIfcObject element)
        {
            this.ifcEntityLabel = element.EntityLabel;
            this.ifcGuid = element.GlobalId;
            this.ifcExpressTypeName = element.GetType().Name;

            if (element.Name != null)
                ifcInfo.Add(new IfcInfo("name", element.Name));
            if (element.Description != null)
                ifcInfo.Add(new IfcInfo("description", element.Description.Value));
            if (element.ObjectType != null)
                ifcInfo.Add(new IfcInfo("ObjectType", element.ObjectType.Value));
        }
    }
}