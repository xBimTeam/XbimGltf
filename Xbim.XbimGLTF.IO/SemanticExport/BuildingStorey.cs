namespace Xbim.GLTF.SemanticExport
{
    public class BuildingStorey : JsonIfcObject
    {
        public string geometryExporterVersion = "1.2";
        public double Elevation;
        public double Height;

        public BuildingStorey(string storeyName, double storeyElevation)
        {
            base.ifcInfo.Add(new IfcInfo("entityName", storeyName));
            Elevation = storeyElevation;
            Height = double.NaN;
        }
    }
}