using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.MeasureResource;
using Xbim.Properties;

namespace Xbim.GLTF.SemanticExport
{
    public class BuildingModelExtractor
    {

        public delegate bool MeshingFilter(int elementId, IModel model);

        /// <summary>
        /// A custom function to determine the behaviour and deflection associated with individual items in the mesher.
        /// Default properties can set in the Model.Modelfactors if the same deflection applies to all elements.
        /// </summary>
        public MeshingFilter CustomFilter;

        /// <summary>
        /// If true, standardised propery sets are extracted to the semantic json file.
        /// </summary>
        public bool IncludeStandardPsets { get; set; } = true;

        /// <summary>
        /// If true, NON standardised propery sets are extracted to the semantic json file.
        /// TODO: NOT IMPLEMENTED.
        /// </summary>
        public bool IncludeNonStandardPsets { get; set; } = false;


        public BuildingModel GetModel(IModel model, IIfcBuildingStorey ignored = null)
        {
            Definitions<PropertySetDef> propertyDefinitions = null;
            // prepare standard properties dictionary
            if (IncludeStandardPsets)
            {
                switch (model.Header.SchemaVersion.ToLowerInvariant())
                {
                    case "ifc2x3":
                        propertyDefinitions = new Definitions<PropertySetDef>(Xbim.Properties.Version.IFC2x3);
                        break;
                    case "ifc4":
                        propertyDefinitions = new Definitions<PropertySetDef>(Xbim.Properties.Version.IFC4);
                        break;
                    default:
                        break;
                }
                if (propertyDefinitions != null)
                    propertyDefinitions.LoadAllDefault();
            }

            // cache systems list
            //
            var elementToSystem = new Dictionary<int, int>();
            var systemRels = model.Instances.OfType<IIfcRelAssignsToGroup>().Where(x =>
                    x.RelatingGroup is IIfcSystem
                    );
            foreach (var systemRel in systemRels)
            {
                foreach (var relatedObject in systemRel.RelatedObjects)
                {
                    if (elementToSystem.ContainsKey(relatedObject.EntityLabel))
                        continue;
                    elementToSystem.Add(relatedObject.EntityLabel, systemRel.RelatingGroup.EntityLabel);
                }
            }

            // now extract
            BuildingModel m = new BuildingModel();

            List<IIfcBuildingStorey> storeys = new List<IIfcBuildingStorey>();
            List<IIfcSystem> systems = new List<IIfcSystem>();

            foreach (var ifcElement in model.Instances.OfType<IIfcElement>())
            {
                if (CustomFilter != null)
                {
                    var skip = CustomFilter(ifcElement.EntityLabel, model);
                    if (skip)
                        continue;
                }

                BuildingElement semanticElement = new BuildingElement();
                semanticElement.SetBase(ifcElement);

                if (propertyDefinitions != null)
                {
                    var thisClass = new ApplicableClass();
                    thisClass.ClassName = ifcElement.ExpressType.Name;
                    var applicable = propertyDefinitions.DefinitionSets.Where(x => x.ApplicableClasses.Any(ac => ac.ClassName == thisClass.ClassName));

                    var psets = ifcElement.IsDefinedBy.Where(x => x.RelatingPropertyDefinition is IIfcPropertySet).Select(pd => pd.RelatingPropertyDefinition as IIfcPropertySetDefinition).ToList();
                    foreach (var definition in applicable)
                    {
                        var matchingSet = psets.Where(x => x.Name == definition.Name).FirstOrDefault();
                        if (matchingSet == null)
                            continue;
                        var ps = new ElementPropertySet();
                        ps.propertySetName = matchingSet.Name;

                        foreach (var singleProperty in definition.PropertyDefinitions)
                        {
                            var pFound = GetProperty(matchingSet, singleProperty);
                            if (!string.IsNullOrWhiteSpace(pFound))
                            {
                                ps.properties.Add(new IfcInfo(
                                    singleProperty.Name,
                                    pFound
                                    ));
                            }
                        }
                        if (ps.properties.Any())
                            semanticElement.propertySets.Add(ps);
                    }
                }

                // storeys (prepares list and sets index, data extraction happens later)
                semanticElement.ifcStoreyIndex = GetStoreyId(ifcElement, storeys);
                if (semanticElement.ifcStoreyIndex == -1)
                {
                    // try through upper level of aggregation (up from the aggregates to the RelatingObject)
                    //
                    foreach (var relAggreg in ifcElement.Decomposes.OfType<IIfcRelAggregates>())
                    {
                        int found = GetStoreyId(relAggreg.RelatingObject as IIfcElement, storeys);
                        if (found != -1)
                        {
                            semanticElement.ifcStoreyIndex = found;
                            break;
                        }
                    }
                }

                // systems (prepares list and sets index, data extraction happens later)
                if (elementToSystem.ContainsKey(ifcElement.EntityLabel))
                {
                    var systemId = elementToSystem[ifcElement.EntityLabel];
                    var system = model.Instances[systemId] as IIfcSystem;
                    if (system != null)
                    {
                        int index = systems.IndexOf(system);
                        if (index == -1)
                        {
                            index = systems.Count;
                            systems.Add(system);
                        }
                        semanticElement.ifcSystemIndex = index;
                    }
                }

                // now add element
                m.elements.Add(semanticElement);
            }

            // data extraction for the dicionaries happens here
            foreach (var storey in storeys)
            {
                var s = new BuildingStorey(
                    ToDouble(storey.Elevation)
                    );
                s.SetBase(storey);
                m.storeys.Add(s);
            }

            foreach (var system in systems)
            {
                var s = new BuildingSystem();
                s.SetBase(system);
                m.systems.Add(s);
            }

            return m;
        }

        private static int GetStoreyId(IIfcElement ifcElement, List<IIfcBuildingStorey> storeys)
        {
            if (ifcElement == null)
                return -1;
            var ret = -1;
            foreach (var rel in ifcElement.ContainedInStructure)
            {
                if (rel.RelatingStructure is IIfcBuildingStorey storey)
                {
                    int index = storeys.IndexOf(storey);
                    if (index == -1)
                    {
                        index = storeys.Count;
                        storeys.Add(storey);
                    }
                    ret = index;
                }
            }
            return ret;
        }

        private string GetProperty(IIfcPropertySetDefinition matchingSet, PropertyDef singleProperty)
        {
            var asSet = matchingSet as IIfcPropertySet;
            if (asSet != null)
            {
                var found = asSet.HasProperties.OfType<IIfcPropertySingleValue>().FirstOrDefault(x => x.Name == singleProperty.Name);
                if (found != null)
                    return found.NominalValue.ToString();
                foreach (var alias in singleProperty.NameAliases)
                {
                    found = asSet.HasProperties.OfType<IIfcPropertySingleValue>().FirstOrDefault(x => x.Name == singleProperty.Name);
                    if (found != null)
                        return found.NominalValue.ToString();
                }
            }
            return "";
        }

        private double ToDouble(IfcLengthMeasure? totalHeight)
        {
            if (!totalHeight.HasValue)
                return double.NaN;
            return (double)totalHeight.Value.Value;

        }
    }
}
