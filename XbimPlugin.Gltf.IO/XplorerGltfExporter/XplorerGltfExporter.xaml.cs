using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.GLTF;
using Xbim.GLTF.ExportHelpers;
using Xbim.Ifc;
using Xbim.Presentation;
using Xbim.Presentation.XplorerPluginSystem;

namespace Xbim.Gltf
{
    /// <summary>
    /// Interaction logic for XplorerPlugin.xaml
    /// </summary>
    [XplorerUiElement(PluginWindowUiContainerEnum.LayoutAnchorable, PluginWindowActivation.OnMenu, "Gltf Exporter")]
    public partial class XplorerGltfExporter : System.Windows.Controls.UserControl, IXbimXplorerPluginWindow
    {
        // internal static ILogger Logger { get; private set; }


        public XplorerGltfExporter()
        {
            InitializeComponent();
        }

        public string WindowTitle => "Xbim.Gltf Exporter";

        private IXbimXplorerPluginMasterWindow _xpWindow;


        // Selection
        public EntitySelection Selection
        {
            get { return (EntitySelection)GetValue(SelectionProperty); }
            set { SetValue(SelectionProperty, value); }
        }

        public static DependencyProperty SelectionProperty =
            DependencyProperty.Register(
                "Selection", 
                typeof(EntitySelection), 
                typeof(XplorerGltfExporter), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnPropertyChanged)
                );

        // SelectedEntity
        public IPersistEntity SelectedEntity
        {
            get { return (IPersistEntity)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedEntity", 
                typeof(IPersistEntity), 
                typeof(XplorerGltfExporter), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnPropertyChanged)
                );

        // Model
        public IModel Model
        {
            get { return (IModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static DependencyProperty ModelProperty =
            DependencyProperty.Register(
                "Model", 
                typeof(IModel), 
                typeof(XplorerGltfExporter), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnPropertyChanged)
                );

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public void BindUi(IXbimXplorerPluginMasterWindow mainWindow)
        {
            _xpWindow = mainWindow;
            SetBinding(SelectedItemProperty, new Binding("SelectedItem") { Source = mainWindow, Mode = BindingMode.TwoWay });
            SetBinding(SelectionProperty, new Binding("Selection") { Source = mainWindow.DrawingControl, Mode = BindingMode.TwoWay });
            SetBinding(ModelProperty, new Binding()); // whole datacontext binding, see http://stackoverflow.com/questions/8343928/how-can-i-create-a-binding-in-code-behind-that-doesnt-specify-a-path
            
            // versioning information
            //
            var assembly = Assembly.GetAssembly(typeof(Xbim.GLTF.Builder));
            PluginVersion.Text = $"Assembly Version: {assembly.GetName().Version}";
            if (_xpWindow == null)
                return;
            var location = _xpWindow.GetAssemblyLocation(assembly);
            var fvi = FileVersionInfo.GetVersionInfo(location);
            PluginVersion.Text += $"\r\nFile Version: {fvi.FileVersion}";

            // Logger = _xpWindow.LoggerFactory.CreateLogger("XbimXplorer.Commands.QueryEngine");
        }


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // if any UI event should happen it needs to be specified here
            var window = d as XplorerGltfExporter;
            if (window == null)
                return;

            switch (e.Property.Name)
            {
                case "Selection":
                    Debug.WriteLine(e.Property.Name + @" changed");
                    break;
                case "Model":
                    Debug.WriteLine(e.Property.Name + @" changed");
                    break;
                case "SelectedEntity":
                    Debug.WriteLine(e.Property.Name + @" changed");
                    break;
            }
        }

        private void ExportSingle_Click(object sender, RoutedEventArgs e)
        {
            IfcStore s = Model as IfcStore;
            if (s == null || string.IsNullOrEmpty(s.FileName))
            {
                MessageBox.Show("Please save the model in xbim format before exporting.");
                return;
            }


            // ILog Log = LogManager.GetLogger("Xbim.Gltf.XplorerGltfExporter");

            var curr = this.Cursor;
            Cursor = System.Windows.Input.Cursors.Wait;
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                var savename = Path.ChangeExtension(s.FileName, ".gltf");
                var bldr = new Builder();
                var ret = bldr.BuildInstancedScene(Model, XbimMatrix3D.Identity);
                glTFLoader.Interface.SaveModel(ret, savename);

                // Log.Info($"Gltf Model exported to '{savename}' in {sw.ElapsedMilliseconds} ms.");
                FileInfo f = new FileInfo(s.FileName);

                // write json
                //
                var jsonFileName = Path.ChangeExtension(s.FileName, "json");
                var bme = new GLTF.SemanticExport.BuildingModelExtractor();
                var rep = bme.GetModel(s);
                rep.Export(jsonFileName);

                // decide if showing the model.
                //
                var answ = MessageBox.Show("File created, do you want to show it in windows explorer?", "Completed", MessageBoxButton.YesNo);
                if (answ == MessageBoxResult.Yes)
                    SelectFile(savename);

                //System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                //{
                //    FileName = f.DirectoryName,
                //    UseShellExecute = true,
                //    Verb = "open"
                //});
            }
            catch (System.Exception err)
            {
                // Log.Error("Error exporting gltf, see inner exception for details.", err);
            }
            Cursor = curr;

        }

        // protected Microsoft.Extensions.Logging.ILogger Log { get; set; } = 

        private void ExportMultiple_Click(object sender, RoutedEventArgs e)
        {
            IfcStore s = Model as IfcStore;
            if (s == null || string.IsNullOrEmpty(s.FileName))
            {
                MessageBox.Show("Please save the model in xbim format before exporting.");
                return;
            }

            
            var curr = this.Cursor;
            Cursor = System.Windows.Input.Cursors.Wait;
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                var exp = new MultipleFilesExporter();
                IfcStore str = Model as IfcStore;
                if (str != null)
                    exp.ExportByStorey(str, ExportSemantic.IsChecked.Value);

                var answ = MessageBox.Show("Files created, do you want to open the folder in windows explorer?", "Completed", MessageBoxButton.YesNo);
                if (answ == MessageBoxResult.Yes)
                {
                    FileInfo f = new FileInfo(str.FileName);
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName =  f.DirectoryName,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
            }
            catch (Exception err)
            {
                // Log?.Error("Error exporting gltf, see inner exception for details.", err);
            }
            Cursor = curr; 
        }

        private void SelectFile(string fullName)
        {
            if (!File.Exists(fullName))
            {
                return;
            }
            // combine the arguments together
            // it doesn't matter if there is a space after ','
            string argument = "/select, \"" + fullName + "\"";
            Process.Start("explorer.exe", argument);
        }
    }
}
