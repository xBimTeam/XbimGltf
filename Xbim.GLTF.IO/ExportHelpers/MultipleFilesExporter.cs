using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.GLTF.SemanticExport;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.MeasureResource;
using Xbim.ModelGeometry.Scene;

namespace Xbim.GLTF.ExportHelpers
{
    public class MultipleFilesExporter
    {
        int[] elemsToExport;

        bool Filter(int elementId, IModel model)
        {
            return !elemsToExport.Contains(elementId);
        }

        public void ExportByStorey(string fileName)
        {
            FileInfo f = new FileInfo(fileName);
            
            var ifcName = Path.ChangeExtension(fileName, "ifc");
            using (var store = IfcStore.Open(ifcName))
            {
                ExportByStorey(store);
            }
        }

        public void ExportByStorey(IfcStore store, bool exportSemantic = true)
        {
            var f = new FileInfo(store.FileName);
            var dir = f.Directory;
            if (store.GeometryStore.IsEmpty)
            {
                var context = new Xbim3DModelContext(store);
                context.CreateContext();
            }

            foreach (var storey in store.Instances.OfType<IIfcBuildingStorey>())
            {
                // prepare filter
                var rels = store.Instances.OfType<IIfcRelContainedInSpatialStructure>().Where(x => x.RelatingStructure.EntityLabel == storey.EntityLabel);
                List<int> els = new List<int>();
                foreach (var rel in rels)
                {
                    // entities directly in the relation
                    //
                    var entitiesInStoreyRel = rel.RelatedElements.Select(x => x.EntityLabel).ToList();
                    els.AddRange(entitiesInStoreyRel);

                    // decomposed elements
                    // 
                    var relsToComposingEntities = store.Instances.OfType<IIfcRelAggregates>().Where(x => entitiesInStoreyRel.Contains(x.RelatingObject.EntityLabel));
                    foreach (var relToComposingEntities in relsToComposingEntities)
                    {
                        els.AddRange(relToComposingEntities.RelatedObjects.Select(x => x.EntityLabel).ToList());  
                    }
                }

                // only export once
                elemsToExport = els.Distinct().ToArray();

                // write gltf
                //
                var bldr = new Builder();
                bldr.BufferInBase64 = true;
                bldr.CustomFilter = this.Filter;

                var storeyName = storey.Name.ToString();
                if (HasInvalidChars(storeyName))
                {
                    storeyName = storey.EntityLabel.ToString();
                }
                
                var outName = Path.Combine(
                    dir.FullName,
                    f.Name + "." + storeyName + ".gltf"
                    );
                var ret = bldr.BuildInstancedScene(store, XbimMatrix3D.Identity);
                if (ret != null && exportSemantic)
                {
                    // actual write if not empty model.
                    //
                    glTFLoader.Interface.SaveModel(ret, outName);

                    // write json
                    //
                    var jsonFileName = Path.ChangeExtension(outName, "json");
                    var bme = new BuildingModelExtractor();
                    bme.CustomFilter = this.Filter;
                    var rep = bme.GetModel(store);
                    rep.Export(jsonFileName);
                }
            }
        }

        private bool HasInvalidChars(string storeyName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var broken = storeyName.Split(invalid);
            return (broken.Length != 1);
        }
    }
}
