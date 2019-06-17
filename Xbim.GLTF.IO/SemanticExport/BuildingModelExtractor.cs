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

        public bool IncludeStandardPsets { get; set; } = true;

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
                        propertyDefinitions = new Definitions<PropertySetDef>(Xbim.Properties.Version.IFC2x3);
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

            foreach (var element in model.Instances.OfType<IIfcElement>())
            {
                if (CustomFilter != null)
                {
                    var skip = CustomFilter(element.EntityLabel, model);
                    if (skip)
                        continue;
                }

                BuildingElement el = new BuildingElement();
                el.SetBase(element);

                if (propertyDefinitions != null)
                {
                    var thisClass = new ApplicableClass();
                    thisClass.ClassName = element.ExpressType.Name;
                    var applicable = propertyDefinitions.DefinitionSets.Where(x => x.ApplicableClasses.Any(ac=>ac.ClassName == thisClass.ClassName));

                    var psets = element.IsDefinedBy.Where(x => x.RelatingPropertyDefinition is IIfcPropertySet).Select(pd => pd.RelatingPropertyDefinition as IIfcPropertySetDefinition).ToList();
                    foreach (var definition in applicable)
                    {
                        var matchingSet = psets.Where(x => x.Name == definition.Name).FirstOrDefault();
                        if (matchingSet == null)
                            continue;
                        ElementPropertySet ps = new ElementPropertySet();
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
                            el.propertySets.Add(ps);
                    }
                }
                // storeys (prepares list and sets index, data extraction happens later)
                foreach (var rel in element.ContainedInStructure)
                {
                    if (rel.RelatingStructure is IIfcBuildingStorey storey)
                    {
                        int index = storeys.IndexOf(storey);
                        if (index == -1)
                        {
                            index = storeys.Count;
                            storeys.Add(storey);
                        }
                        el.ifcStoreyIndex = index;
                    }                    
                }

                // systems (prepares list and sets index, data extraction happens later)
                if (elementToSystem.ContainsKey(element.EntityLabel))
                {
                    var systemId = elementToSystem[element.EntityLabel];
                    var system = model.Instances[systemId] as IIfcSystem;
                    if (system != null)
                    {
                        int index = systems.IndexOf(system);
                        if (index == -1)
                        {
                            index = systems.Count;
                            systems.Add(system);
                        }
                        el.ifcSystemIndex = index;
                    }
                }
                
                // now add element
                m.elements.Add(el);
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
