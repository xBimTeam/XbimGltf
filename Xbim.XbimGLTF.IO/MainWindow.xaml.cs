using glTFLoader.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using User;
using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Common.Metadata;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace Xbim.XbimGLTF.IO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var inValue = new byte[] {
                1, 0, 0
            };
            var str = Convert.ToBase64String(inValue);
            Debug.WriteLine(str);

            byte[] databack = Convert.FromBase64String(str);

            for (int i = 0; i < databack.Length; i++)
            {
                if (inValue[i] != databack[i])
                {
                    Debug.WriteLine("error.");
                }
            }

            var example = "AAABAAIAAAAAAAAAAAAAAAAAAAAAAIA/AAAAAAAAAAAAAAAAAACAPwAAAAA=";
            databack = Convert.FromBase64String(example);
            for (int i = 0; i < databack.Length; i++)
            {
                Debug.Write($", {databack[i]} ");
            }
            Debug.WriteLine("");
            Debug.WriteLine($"Len: {databack.Length}");


            str = Convert.ToBase64String(test);

        }

        private byte[] test = new byte[]
           {
                0, 0, 1, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 0, 0, 0
           };

        private void MakeTriangle_Click(object sender, RoutedEventArgs e)
        {
            //var s = new Gltf2Schema();
            //s.buffers.Add(new Buffer(test));


            //JsonSerializer serializer = new JsonSerializer();
            //serializer.NullValueHandling = NullValueHandling.Ignore;
            //using (StreamWriter sw = new StreamWriter("out.gltf"))
            //using (JsonWriter writer = new JsonTextWriter(sw))
            //{
            //    serializer.Serialize(writer, s);
            //}


        }

        private void libtest_Click(object sender, RoutedEventArgs e)
        {
            var t = glTFLoader.Interface.LoadModel("..\\..\\..\\Resources\\OneWall.gltf");

            report(t);

            var gltf = CreateModel();
            glTFLoader.Interface.SaveModel(gltf, "Data\\BoxInterleaved2.gltf");
        }

        private void report(Gltf t)
        {
            var sb = new StringBuilder();
            ReportAllProperties(t, sb);
            Debug.WriteLine(sb.ToString());
            // PrintProperties(t, 0);
        }

        public static string ReportAllProperties<T>(T instance, StringBuilder sb, int indent = 0) where T : class
        {
            if (instance == null)
                return string.Empty;

            var strListType = typeof(List<string>);
            var strArrType = typeof(string[]);



            var arrayTypes = new[] { strListType, strArrType };
            var handledTypes = new[] {
                typeof(bool),
                typeof(Int32),
                typeof(String),
                typeof(DateTime),
                typeof(double),
                typeof(decimal),
                typeof(float),
                strListType,
                strArrType
            };

            try
            {
                

                var ind = new string('\t', indent);

                
                var elems = instance as System.Collections.ICollection;
                if (elems != null)
                {
                    sb.AppendLine(ind + "===");

                    int i = 0;
                    if (elems.Count == 16)
                    {
                        sb.Append(ind);
                        foreach (var elem in elems)
                        {
                            sb.Append(elem + "\t");
                            i++;
                            if (i==4  || i  == 8 || i == 12)
                            {
                                sb.AppendLine("");
                                sb.Append(ind);
                            }
                        }
                        sb.AppendLine("");
                    }
                    else {
                        
                        foreach (var elem in elems)
                        {
                            if (handledTypes.Contains(elem.GetType()))
                            {
                                sb.AppendLine(ind + $"( {i++} ) '{elem}'");
                            }
                            else
                            {
                                sb.AppendLine(ind + $"( {i++} ) [{elem.GetType().Name}]");
                                ReportAllProperties(elem, sb, indent + 1);
                            }
                        }
                    }
                }
                else
                {
                    var validProperties = instance.GetType()
                                              .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                              // .Where(prop => handledTypes.Contains(prop.PropertyType))
                                              .Where(prop => prop.GetValue(instance, null) != null)
                                              .ToList();
                    if (!validProperties.Any())
                        return "";
                    var format = ind + string.Format("{{0,-{0}}} : {{1}}", validProperties.Max(prp => prp.Name.Length));
                    foreach (var prop in validProperties)
                    {
                        if (prop.Name == "SyncRoot")
                            continue;
                        var itervalue = prop.GetValue(instance, null);
                        var outv = string.Format(format, prop.Name, (arrayTypes.Contains(prop.PropertyType) ? string.Join(", ", (IEnumerable<string>)itervalue) : prop.GetValue(instance, null)));
                        sb.AppendLine(outv);
                        if (!handledTypes.Contains(prop.PropertyType))
                        {
                            ReportAllProperties(itervalue, sb, indent + 1);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            return "";
            
        }

        public void PrintProperties(object obj, int indent)
        {
            if (obj == null) return;
            string indentString = new string(' ', indent);
            Type objType = obj.GetType();
            PropertyInfo[] properties = objType.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object propValue = property.GetValue(obj, null);
                var elems = propValue as System.Collections.IList;
                if (elems != null)
                {
                    foreach (var item in elems)
                    {
                        PrintProperties(item, indent + 3);
                    }
                }
                else
                {
                    // This will not cut-off System.Collections because of the first check
                    if (property.PropertyType.Assembly == objType.Assembly)
                    {
                        Debug.WriteLine("{0}{1}:", indentString, property.Name);

                        PrintProperties(propValue, indent + 2);
                    }
                    else
                    {
                        Debug.WriteLine("{0}{1}: {2}", indentString, property.Name, propValue);
                    }
                }
            }
        }

        private static glTFLoader.Schema.Gltf CreateModel()
        {
            glTFLoader.Schema.Gltf gltf = new glTFLoader.Schema.Gltf();
            gltf.Asset = new glTFLoader.Schema.Asset()
            {
                Version = "2.0",
                Generator = "Xbim.GLTF.IO"
            };
            return gltf;
        }

        private void GetModel(object sender, RoutedEventArgs e)
        {
            using (var m = IfcStore.Open(@"C:\Users\Claudio\Dev\XbimTeam\XbimWebUI\Xbim.WeXplorer\tests\data\OneWall.ifc"))
            {
                var c = new Xbim3DModelContext(m);
                c.CreateContext();
                m.SaveAs("model.xbim");
            }
        }

        IfcStore _model = null;

        private void OpenModel(object sender, RoutedEventArgs e)
        {
            if (_model != null)
            {
                _model.Close();
                _model.Dispose();
            }
            _model = IfcStore.Open("model.xbim");
        }

        public static HashSet<short> DefaultExclusions(IModel model, List<Type> exclude)
        {
            var excludedTypes = new HashSet<short>();
            if (exclude == null)
                exclude = new List<Type>()
                {
                    typeof(IIfcSpace),
                    typeof(IIfcFeatureElement)
                };
            foreach (var excludedT in exclude)
            {
                ExpressType ifcT;
                if (excludedT.IsInterface && excludedT.Name.StartsWith("IIfc"))
                {
                    var concreteTypename = excludedT.Name.Substring(1).ToUpper();
                    ifcT = model.Metadata.ExpressType(concreteTypename);
                }
                else
                    ifcT = model.Metadata.ExpressType(excludedT);
                if (ifcT == null) // it could be a type that does not belong in the model schema
                    continue;
                foreach (var exIfcType in ifcT.NonAbstractSubTypes)
                {
                    excludedTypes.Add(exIfcType.TypeId);
                }
            }
            return excludedTypes;
        }

        static readonly XbimColourMap _colourMap = new XbimColourMap();

        private static List<double> GetTypeColour(IModel model, short ifcTypeId)
        {
            var prodType = model.Metadata.ExpressType(ifcTypeId);
            var colour = _colourMap[prodType.Name];
            return new List<double>()
            {
                colour.Red, colour.Green, colour.Blue, colour.Alpha
            };
        }

        private static List<double> GetStyleColour(IModel model, int styleId)
        {
            var sStyle = model.Instances[styleId] as IIfcSurfaceStyle;
            var texture = XbimTexture.Create(sStyle);
            if (texture.ColourMap.Any())
            {
                var colour = texture.ColourMap[0];
                return new List<double>()
                {
                    colour.Red, colour.Green, colour.Blue, colour.Alpha
                };
            }
            return null;
        }

        static private IEnumerable<XbimShapeInstance> GetShapeInstancesToRender(IGeometryStoreReader geomReader, HashSet<short> excludedTypes)
        {
            var shapeInstances = geomReader.ShapeInstances
                .Where(s => s.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded
                            &&
                            !excludedTypes.Contains(s.IfcTypeId));
            return shapeInstances;
        }

        public static void BuildInstancedScene(IModel model, List<Type> exclude = null, bool WantRefresh = false)
        {
            var gltf = CreateModel();

            Dictionary<int, object> osgGeoms = new Dictionary<int, object>();

            // this needs to open a previously meshed xbim file.
            //
            Dictionary<int, List<double>> styleDic = new Dictionary<int, List<double>>();

            List<XbimMesher> mshs = new List<XbimMesher>();
            
            var s = new Stopwatch();
            s.Start();
            int iCnt = 0;
            Random r = new Random();
            var excludedTypes = DefaultExclusions(model, exclude);
            using (var geomStore = model.GeometryStore)
            using (var geomReader = geomStore.BeginRead())
            {
                var sstyleIds = geomReader.StyleIds;
                foreach (var styleId in sstyleIds)
                {
                    var color = GetStyleColour(model, styleId);
                    if (color != null)
                        styleDic.Add(styleId, color);
                }

                var shapeInstances = GetShapeInstancesToRender(geomReader, excludedTypes);
                // foreach (var shapeInstance in shapeInstances.OrderBy(x=>x.IfcProductLabel))
                foreach (var shapeInstance in shapeInstances.OrderBy(x => x.IfcProductLabel))
                {
                    IXbimShapeGeometryData shapeGeom = geomReader.ShapeGeometry(shapeInstance.ShapeGeometryLabel);
                    if (shapeGeom.Format != (byte)XbimGeometryType.PolyhedronBinary)
                        continue;
                    
                    // work out colour id
                    // positives are styles, negatives are types
                    var colId = shapeInstance.StyleLabel > 0
                        ? shapeInstance.StyleLabel
                        : shapeInstance.IfcTypeId * -1;

                    List<double> color = null;
                    if (!styleDic.TryGetValue(colId, out color))
                    {
                        // if the style is not available we build one by ExpressType
                        var mg = GetTypeColour(model, shapeInstance.IfcTypeId);
                        styleDic.Add(colId, mg);
                        if (mg != null)
                            color = mg;
                    }

                    if (color == null)
                    {
                        color = new List<double>();
                        color.Add(r.NextDouble());
                        color.Add(r.NextDouble());
                        color.Add(r.NextDouble());
                        color.Add(1);
                    }

                    if (false && shapeGeom.ReferenceCount > 1)
                    {
                        // repeat the map multiple times
                        //
                        // XbimGeom osgGeom = null;
                        // if g is not found in the dictionary then build it and add it
                        object osgGeom;
                        if (!osgGeoms.TryGetValue(shapeGeom.ShapeLabel, out osgGeom))
                        {
                            // mesh once
                            var xbimMesher = new XbimMesher();
                            xbimMesher.AddMesh(shapeGeom.ShapeData);

                            // todo: add to the model

                            //osgGeom = osgControl.AddGeom(
                            //    xbimMesher.PositionsAsDoubleList(model.ModelFactors.OneMeter),
                            //    xbimMesher.Indices,
                            //    xbimMesher.NormalsAsDoubleList(),
                            //    color
                            //    );
                            osgGeoms.Add(shapeGeom.ShapeLabel, osgGeom);
                        }

                        if (osgGeom != null)
                        {
                            var arr = shapeInstance.Transformation.ToDoubleArray();
                            arr[12] /= model.ModelFactors.OneMeter;
                            arr[13] /= model.ModelFactors.OneMeter;
                            arr[14] /= model.ModelFactors.OneMeter;

                            // todo: add to the model
                            // var osgTransform = osgControl.AddTransform(osgGeom, ref arr);

                            // geodes.Add(osgGeode);
                        }
                    }
                    else
                    {
                        // repeat the geometry only once
                        //
                        var xbimMesher = new XbimMesher();
                        xbimMesher.AddShape(geomReader, shapeInstance);
                        mshs.Add(xbimMesher);

                        
                        // todo: add to the model

                        //var osgGeode = osgControl.AddGeode(
                        //    xbimMesher.PositionsAsDoubleList(model.ModelFactors.OneMeter),
                        //    xbimMesher.Indices,
                        //    xbimMesher.NormalsAsDoubleList(),
                        //    color
                        //    );

                    }
                    // frame refresh
                    iCnt++;
                }
            }

            AddMesh(gltf, mshs[0]);

            Debug.WriteLine($"added {iCnt} elements in {s.ElapsedMilliseconds}ms.");
        }

        private static void AddMesh(Gltf gltf, XbimMesher mesh)
        {
            gltf.Buffers = new glTFLoader.Schema.Buffer[1];
            gltf.Buffers[0] = new glTFLoader.Schema.Buffer();
            gltf.BufferViews = new BufferView[2];
            
        }

        private void TryMesh(object sender, RoutedEventArgs e)
        {
            BuildInstancedScene(_model);
        }
    }
}
