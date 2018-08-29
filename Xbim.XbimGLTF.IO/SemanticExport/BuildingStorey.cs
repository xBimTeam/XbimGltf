namespace Xbim.GLTF.SemanticExport
{
    public class BuildingStorey : JsonIfcObject
    {
        public string geometryExporterVersion = "1.2";

        /// <summary>
        /// Elevation of the base of this storey, relative to the 0,00 internal reference height of the building. The 0.00 level is given by the absolute above sea level height by the ElevationOfRefHeight attribute given at IfcBuilding.
        /// </summary>
        public double Elevation;
        

        public BuildingStorey(double storeyElevation)
        {
            Elevation = storeyElevation;            
        }
    }
}