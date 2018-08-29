using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xbim.GLTF.SemanticExport
{
    public class BuildingModel
    {
        public string geometryExporterVersion = "1.2";

        public string uniZiteMetadataExporterVersion = "1.2";

        public string ifcSchemaVersion = "ifc2x3";

        public List<BuildingElement> elements = new List<BuildingElement>();
        
        public List<BuildingSystem> systems = new List<BuildingSystem>();

        public List<BuildingStorey> storeys = new List<BuildingStorey>();

        public FileInfo Export(string v)
        {
            FileInfo f = new FileInfo(v);
            if (f.Exists)
                f.Delete();
            using (var stream = f.OpenWrite())
            {
                SerializeToStream(this, stream);
            }
            return f;
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings{
                Converters = new List<JsonConverter> {
                    new StringEnumConverter()
                    }
                };
            return settings;
        }

        public static void SerializeToStream(BuildingModel obj, Stream stream)
        {
            var settings = GetJsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            var serializer = JsonSerializer.Create(settings);

            using (var sw = new StreamWriter(stream))
            using (var jsonTextWriter = new JsonTextWriter(sw))
            {
                serializer.Serialize(jsonTextWriter, obj);
            }
        }

        public static BuildingModel DeserializeFromStream(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                var obj = serializer.Deserialize(jsonTextReader, typeof(BuildingModel));
                return obj as BuildingModel;
            }
        }
    }
}
