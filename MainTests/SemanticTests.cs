using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xbim.GLTF.SemanticExport;

namespace MainTests
{
    [TestClass]
    public class SemanticTests
    {
        [TestMethod]
        public void SemanticExportTest()
        {
            // create file
            //
            BuildingModel m = new BuildingModel();
            m.storeys.Add(new BuildingStorey(10.3));
            var fi = m.Export("file.json");

            // attempt reload
            //
            using (var readStream = fi.OpenRead())
            {
                var b = BuildingModel.DeserializeFromStream(readStream);
                var readElev = b.storeys.FirstOrDefault()?.Elevation;
                Assert.AreEqual(10.3, readElev);
            }
        }
    }
}
