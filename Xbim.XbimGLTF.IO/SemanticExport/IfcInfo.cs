namespace Xbim.GLTF.SemanticExport
{
    public class IfcInfo
    {
        public string Name;
        public string Value;

        // e.g. 
        //    "objectType":,
        //    "entityName:" ,
        //    "entityDescription:" ,

        public IfcInfo(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}