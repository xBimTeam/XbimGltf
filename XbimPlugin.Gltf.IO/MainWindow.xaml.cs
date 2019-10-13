using gltf = glTFLoader.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.ModelGeometry.Scene;
using Xbim.Geom;
using Xbim.Ifc4.Interfaces;
using Xbim.Common;
using Xbim.GLTF.SemanticExport;
using Xbim.GLTF.ExportHelpers;

namespace Xbim.GLTF
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
            var t = glTFLoader.Interface.LoadModel("..\\..\\..\\Resources\\TwoWallsTwoColour.gltf");

            report(t);

            // var gltf = CreateModel();
            // glTFLoader.Interface.SaveModel(gltf, "Data\\BoxInterleaved2.gltf");
        }

        private void report(gltf.Gltf t)
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

        string _gltfOutName = "";

        
               
        private void OpenModel(object sender, RoutedEventArgs e)
        {
            if (_model != null)
            {
                _model.Close();
                _model.Dispose();
            }
            // _model = IfcStore.Open("model.xbim");
            var modelName = @"C:\Users\Claudio\Dropbox (Northumbria University)\_Courseware\2017-18\Semester 2\KB7038\materials\Duplex\Duplex_A_20110907.xBIM";

            // on mac
            modelName = @"C:\Users\Claudio\Dropbox (Northumbria University)\Projects\uniZite\BIM model\ARK 0-00-A-200-X-01.xbim";
            modelName = @"C:\Users\Claudio\Dropbox (Northumbria University)\Projects\uniZite\BIM model\Hadsel Bygg B VVS.xbim";
            modelName = @"C:\Users\Claudio\Dropbox (Northumbria University)\Projects\uniZite\Style\Hadsel Bygg B VVS.xBIM";
            modelName = @"C:\Users\Claudio\Dropbox (Northumbria University)\Projects\uniZite\BIM model\VAMMA\Vamma12_ARK.xBIM";
            // modelName = @"C:\Users\Claudio\Dropbox (Northumbria University)\Projects\uniZite\BIM model\VAMMA\Vamma12_RIE.xBIM";

            // at uni
            // modelName = @"C:\Users\sgmk2\Dropbox (Northumbria University)\Projects\uniZite\BIM model\ARK 0-00-A-200-X-01.xBIM";
            // modelName = @"C:\Users\sgmk2\Dropbox (Northumbria University)\Projects\uniZite\BIM model\Hadsel Bygg B VVS.xbim";
            modelName = @"C:\Users\sgmk2\Dropbox (Northumbria University)\Projects\uniZite\BIM model\VAMMA\Vamma12_RIE.xBIM";

            _model = IfcStore.Open(modelName);
            _gltfOutName = Path.ChangeExtension(modelName, "gltf");

        }

        //private static void AddMesh(gltf.Gltf gltf, XbimMesher mesh)
        //{
        //    gltf.Buffers = new glTFLoader.Schema.Buffer[1];
        //    var buf = new glTFLoader.Schema.Buffer();
        //    gltf.Buffers[0] = buf;
        //    gltf.BufferViews = new gltf.BufferView[2];
        //}

        
        
        private void TryMesh(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Method disabled.");
        }

        private void ToBin(object sender, RoutedEventArgs e)
        {
            var d = new DirectoryInfo(@"..\..\..\Resources\");
            
            var files = d.GetFiles("*.gltf");
            foreach (var file in files)
            {
                var t = glTFLoader.Interface.LoadModel(file.FullName);
                var newName = Path.ChangeExtension(file.FullName, ".txt2");
                glTFLoader.Interface.SaveModel(t, newName);
            }
        }

        private void ToText(object sender, RoutedEventArgs e)
        {
            var d = new DirectoryInfo(@"..\..\..\Resources\");

            var files = d.GetFiles("*.gltfb");
            foreach (var file in files)
            {
                var t = glTFLoader.Interface.LoadModel(file.FullName);
                var newName = Path.ChangeExtension(file.FullName, ".2.gltf");
                glTFLoader.Interface.SaveModel(t, newName);
            }
        }

        private void XbimsRead(object sender, RoutedEventArgs e)
        {
            var d = new DirectoryInfo(@"C:\Users\Claudio\Desktop");
            var files = d.GetFiles("*.xbim", SearchOption.AllDirectories);
            
            foreach (var file in files)
            {
                using (var m = IfcStore.Open(file.FullName))
                {
                    using (var geomStore = m.GeometryStore)
                    using (var geomReader = geomStore.BeginRead())
                    {
                        int sameobject = 0;
                        int notIdentity = 0;
                        int productLabel = 0;
                        XbimMatrix3D t = XbimMatrix3D.Identity;
                        var shapeInstances = geomReader.ShapeInstances.Where(s => s.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded);
                        foreach (var shapeInstance in shapeInstances.OrderBy(x => x.IfcProductLabel))
                        {

                            if (productLabel == shapeInstance.IfcProductLabel)
                            {
                                sameobject++;
                                if (!shapeInstance.Transformation.Equals(t))
                                {
                                    
                                }
                            }
                            productLabel = shapeInstance.IfcProductLabel;
                            t = shapeInstance.Transformation;
                            if (!t.Equals(XbimMatrix3D.Identity))
                            {
                                notIdentity++;
                            }
                        }
                        Debug.WriteLine($"{file.FullName} same:{sameobject} notid:{notIdentity}");
                    }
                    m.Close();
                }
            }
        }

        private void ConvertAll(object sender, RoutedEventArgs e)
        {
            foreach (var item in GetModelList())
            {
                MultipleFilesExporter m = new MultipleFilesExporter();
                m.ExportByStorey(item);
            }
        }

        private IEnumerable<string> GetModelList()
        {
            var folders = new string[]
            {
                @"C:\Users\Claudio\Dropbox (Northumbria University)\Projects\uniZite\",
                @"C:\Users\sgmk2\Dropbox (Northumbria University)\Projects\uniZite\"
            };

            var folder = "";
            foreach (var item in folders)
            {
                if (Directory.Exists(item))
                {
                    folder = item;
                    break;
                }
            }

            var files = new string[]
            {
                @"BIM model\ARK 0-00-A-200-X-01.xbim",
                @"BIM model\Hadsel Bygg B VVS.xbim",
                @"BIM model\VAMMA\Vamma12_ARK.xBIM",
                @"BIM model\VAMMA\Vamma12_RIE.xBIM"
            };

            foreach (var item in files)
            {
                var fname = Path.Combine(folder, item);
                if (File.Exists(fname))
                    yield return fname;
            }
        }
    }
}
