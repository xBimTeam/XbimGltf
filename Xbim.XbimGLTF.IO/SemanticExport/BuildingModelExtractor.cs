using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.MeasureResource;

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

        public BuildingModel GetModel(IModel model)
        {
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
                    var system = model.Instances[element.EntityLabel] as IIfcSystem;
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

            // data extraction happens here
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

        private double ToDouble(IfcLengthMeasure? totalHeight)
        {
            if (!totalHeight.HasValue)
                return double.NaN;
            return (double)totalHeight.Value.Value;

        }
    }
}
