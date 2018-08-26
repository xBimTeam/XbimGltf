using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Xbim.Common;
using Xbim.GLTF;
using Xbim.Presentation;
using Xbim.Presentation.XplorerPluginSystem;

namespace Xbim.Gltf
{
    /// <summary>
    /// Interaction logic for XplorerPlugin.xaml
    /// </summary>
    [XplorerUiElement(PluginWindowUiContainerEnum.LayoutAnchorable, PluginWindowActivation.OnMenu, "Gltf Exporter")]
    public partial class XplorerGltfExporter : IXbimXplorerPluginWindow
    {
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
            DependencyProperty.Register("Selection", typeof(EntitySelection), typeof(XplorerGltfExporter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnPropertyChanged));

        // SelectedEntity
        public IPersistEntity SelectedEntity
        {
            get { return (IPersistEntity)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedEntity", typeof(IPersistEntity), typeof(XplorerGltfExporter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits,
                                                                      OnPropertyChanged));

        // Model
        public IModel Model
        {
            get { return (IModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(IModel), typeof(XplorerGltfExporter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits,
                                                                      OnPropertyChanged));

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Model == null)
                return;
                        
            var bldr = new Builder();
            var ret = bldr.BuildInstancedScene(Model);
                        
            // glTFLoader.Interface.SaveModel(ret, _gltfOutName);
        }
    }
}
