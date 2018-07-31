using gltf = glTFLoader.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;
using System;
using Xbim.Geom;
using System.Diagnostics;
using Xbim.Common.Metadata;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc;
using Xbim.Common.Geometry;

namespace Xbim.GLTF
{
    internal class Builder
    {
        // enumberables
        //
        List<gltf.Accessor> _accessors = new List<gltf.Accessor>();
        List<gltf.BufferView> _bufferViews = new List<gltf.BufferView>();
        List<gltf.Material> _materials = new List<gltf.Material>();
        List<gltf.Mesh> _meshes = new List<gltf.Mesh>();
        List<gltf.Node> _nodes = new List<gltf.Node>();

        // singletons
        gltf.Scene _scene = new gltf.Scene();
        gltf.Buffer _buffer = new gltf.Buffer();
        gltf.Node _topNode = new gltf.Node();

        gltf.BufferView _eabBv; // ELEMENT_ARRAY_BUFFER bufferview
        gltf.BufferView _abBv; // ARRAY_BUFFER bufferview


        public Builder()
        {
            InitMaterials();
            InitScene();
            InitBufferViews();
        }

        private void InitBufferViews()
        {
            _eabBv = new gltf.BufferView();
            _eabBv.Buffer = 0;
            _eabBv.Target = gltf.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER;

            _abBv = new gltf.BufferView();
            _abBv.Buffer = 0;
            _abBv.Target = gltf.BufferView.TargetEnum.ARRAY_BUFFER;
            _abBv.ByteStride = 12; // todo: what is this number?
        }

        private void InitScene()
        {
            _scene = new gltf.Scene();
            _scene.Nodes = new[] { 0 };

            _topNode = new glTFLoader.Schema.Node();
            _topNode.Name = "Z_UP";
            _topNode.Matrix = new[]
            {
                1.0f,   0.0f,   0.0f,   0.0f,
                0.0f,   0.0f,  -1.0f,   0.0f,
                0.0f,   1.0f,   0.0f,   0.0f,
                0.0f,   0.0f,   0.0f,   1.0f
            };
        }

        private void InitMaterials()
        {
            // default material is always set at index 0;
            float grey = 0.8f;
            float alpha = 1.0f;
            gltf.Material m = CreateMaterial("Default material", grey, grey, grey, alpha);
            _materials.Add(m);
        }

        private static gltf.Material CreateMaterial(string name, float red, float greeen, float blue, float alpha)
        {
            gltf.Material m = new glTFLoader.Schema.Material();
            m.Name = name;
            m.PbrMetallicRoughness = new glTFLoader.Schema.MaterialPbrMetallicRoughness()
            {
                BaseColorFactor = new Single[] { red, greeen, blue, alpha },
                MetallicFactor = 0,
                RoughnessFactor = 1
            };
            m.EmissiveFactor = new Single[] { 0, 0, 0 };
            m.AlphaMode = (alpha < 1.0f) 
                ? glTFLoader.Schema.Material.AlphaModeEnum.BLEND
                : glTFLoader.Schema.Material.AlphaModeEnum.OPAQUE;
            m.AlphaCutoff = 0.5f;
            m.DoubleSided = false;
            return m;
        }

        internal static HashSet<short> DefaultExclusions(IModel model, List<Type> exclude)
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



        private static glTFLoader.Schema.Gltf CreateModel()
        {
            glTFLoader.Schema.Gltf gltf = new glTFLoader.Schema.Gltf();
            gltf.Asset = new glTFLoader.Schema.Asset()
            {
                Generator = "Xbim.GLTF.IO",
                Version = "0.1"
            };
            return gltf;
        }

        public gltf.Gltf Build()
        {
            var gltf = CreateModel();
            
            // lists
            gltf.Materials = _materials.ToArray();
            if (_accessors.Any())
                gltf.Accessors = _accessors.ToArray();
            if (_bufferViews.Any())
                gltf.BufferViews = _bufferViews.ToArray();
            if (_meshes.Any())
                gltf.Meshes = _meshes.ToArray();
            if (_nodes.Any())
                gltf.Nodes = _nodes.ToArray();

            // singletons
            gltf.Scenes = new glTFLoader.Schema.Scene[] { _scene };
            gltf.Buffers = new glTFLoader.Schema.Buffer[] { _buffer };
            
            gltf.Scene = 0;
            return gltf;
        }

        static readonly XbimColourMap _colourMap = new XbimColourMap();

        private int PrepareTypeMaterial(IModel model, short ifcTypeId)
        {
            var prodType = model.Metadata.ExpressType(ifcTypeId);
            var name = prodType.Name;
            var colour = _colourMap[prodType.Name];

            var ret = _materials.Count;

            _materials.Add(CreateMaterial(name,
                colour.Red,
                colour.Green,
                colour.Blue,
                colour.Alpha
                ));

            return ret;
        }

        Dictionary<int, int> styleDic = new Dictionary<int, int>();

        private void PrepareStyleMaterial(IModel model, int styleId)
        {
            if (styleDic.ContainsKey(styleId))
                return;
            var sStyle = model.Instances[styleId] as IIfcSurfaceStyle;
            var name = sStyle.Name;
            var texture = XbimTexture.Create(sStyle);

            var r = 0.8f;
            var g = 0.8f;
            var b = 0.8f;
            var a = 1.0f;
            
            if (texture.ColourMap.Any())
            {
                var colour = texture.ColourMap[0];
                r = colour.Red;
                g = colour.Green;
                b = colour.Blue;
                a = colour.Alpha;
            }
            else
            {
                // if no color defined and no name, just use default
                if (string.IsNullOrEmpty(name))
                    styleDic.Add(styleId, 0);
            }
            // else add it to dictionary and to list
            styleDic.Add(styleId, _materials.Count);
            _materials.Add(CreateMaterial(name, r, g, b, a));
        }

        static private IEnumerable<XbimShapeInstance> GetShapeInstancesToRender(IGeometryStoreReader geomReader, HashSet<short> excludedTypes)
        {
            var shapeInstances = geomReader.ShapeInstances
                .Where(s => s.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded
                            &&
                            !excludedTypes.Contains(s.IfcTypeId));
            return shapeInstances;
        }
        

        public void BuildInstancedScene(IModel model, List<Type> exclude = null, bool WantRefresh = false)
        {
            Dictionary<int, object> geometries = new Dictionary<int, object>();

            // this needs to open a previously meshed xbim file.
            //
            var s = new Stopwatch();
            s.Start();
            int iCnt = 0;
            Random r = new Random();
            var excludedTypes = DefaultExclusions(model, exclude);
            using (var geomStore = model.GeometryStore)
            using (var geomReader = geomStore.BeginRead())
            {
                // process the materials and styles
                var sstyleIds = geomReader.StyleIds;
                foreach (var styleId in sstyleIds)
                {
                    PrepareStyleMaterial(model, styleId);
                }
                int productLabel = 0;
                var shapeInstances = GetShapeInstancesToRender(geomReader, excludedTypes);
                // foreach (var shapeInstance in shapeInstances.OrderBy(x=>x.IfcProductLabel))
                foreach (var shapeInstance in shapeInstances.OrderBy(x => x.IfcProductLabel))
                {
                    // a product (e.g. wall or window) in the scene returns:
                    // - a node
                    //   - pointing to a mesh, with a transform
                    // - 1 mesh
                    //   - with as many mesh primitives as needed to render the different parts
                    //   - pointers to the a material and accessors as needed
                    // - 3 accessors per primitive
                    //   - vertices, normals, indices
                    // - bufferviews can be reused by different accessors
                    // - data in the buffer, of course

                    if (productLabel != shapeInstance.IfcProductLabel)
                    {
                        // need new product

                        // create node
                        var nodeIndex = _nodes.Count;
                        var entity = model.Instances[shapeInstance.IfcProductLabel] as IIfcProduct;
                        if (entity == null)
                        { // fire error here. 
                        }
                        var tnode = new gltf.Node();
                        tnode.Name = entity.Name  + $" #{entity.EntityLabel}";
                        tnode.Matrix = GetTransformInMeters(model, shapeInstance);
                        
                        // create mesh
                        var meshIndex = _meshes.Count;
                        var mesh = new gltf.Mesh();
                        mesh.Name = "Instance";


                        // link node to mesh
                        tnode.Mesh = meshIndex;
                    }
                    

                    IXbimShapeGeometryData shapeGeom = geomReader.ShapeGeometry(shapeInstance.ShapeGeometryLabel);
                    if (shapeGeom.Format != (byte)XbimGeometryType.PolyhedronBinary)
                        continue;

                    // work out colour id
                    // positives are styles, negatives are types
                    var colId = shapeInstance.StyleLabel > 0
                        ? shapeInstance.StyleLabel
                        : shapeInstance.IfcTypeId * -1;

                    int materialIndex;
                    if (!styleDic.TryGetValue(colId, out materialIndex))
                    {
                        // if the style is not available we build one by ExpressType
                        materialIndex = PrepareTypeMaterial(model, shapeInstance.IfcTypeId);
                        styleDic.Add(colId, materialIndex);
                    }

                    // note: at a first investigation it looks like the shapeInstance.Transformation is the same for all shapes of the same product

                    if (false && shapeGeom.ReferenceCount > 1)
                    {
                        // repeat the map multiple times
                        //
                        // XbimGeom osgGeom = null;
                        // if g is not found in the dictionary then build it and add it
                        object osgGeom;
                        if (!geometries.TryGetValue(shapeGeom.ShapeLabel, out osgGeom))
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
                            geometries.Add(shapeGeom.ShapeLabel, osgGeom);
                        }

                        if (osgGeom != null)
                        {
                            var arr = GetTransformInMeters(model, shapeInstance);
                            
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
                        xbimMesher.AddShape(geomReader, shapeInstance, XbimMesher.CoordinatesMode.IgnoreShapeTransform);
                        var trsf = GetTransformInMeters(model, shapeInstance);


                        // mshs.Add(xbimMesher);


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
            Debug.WriteLine($"added {iCnt} elements in {s.ElapsedMilliseconds}ms.");


        }

        private static float[] GetTransformInMeters(IModel model, XbimShapeInstance shapeInstance)
        {
            var arr = shapeInstance.Transformation.ToFloatArray();
            arr[12] /= (float)model.ModelFactors.OneMeter;
            arr[13] /= (float)model.ModelFactors.OneMeter;
            arr[14] /= (float)model.ModelFactors.OneMeter;
            return arr;
        }
    }
}
