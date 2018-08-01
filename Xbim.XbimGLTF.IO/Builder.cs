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

        gltf.BufferView _indicesBv; // ELEMENT_ARRAY_BUFFER bufferview
        gltf.BufferView _coordinatesBv; // ARRAY_BUFFER bufferview

        List<Byte> _indicesBuffer; // ELEMENT_ARRAY_BUFFER bufferview
        List<Byte> _coordinatesBuffer; // ARRAY_BUFFER bufferview
        
        public Builder()
        {
            InitMaterials();
            InitScene();
            InitBufferViews();
        }

        private void InitBufferViews()
        {
            _indicesBv = new gltf.BufferView();
            _indicesBv.Buffer = 0;
            _indicesBv.Target = gltf.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER;

            _coordinatesBv = new gltf.BufferView();
            _coordinatesBv.Buffer = 0;
            _coordinatesBv.Target = gltf.BufferView.TargetEnum.ARRAY_BUFFER;
            _coordinatesBv.ByteStride = 12; // todo: what is this number?
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

        private class ShapeComponentIds
        {
            public int IndicesAccessorId;
            public int VerticesAccessorId;
            public int NormalsAccessorId;
        }
        

        public void BuildInstancedScene(IModel model, List<Type> exclude = null, bool WantRefresh = false)
        {
            Dictionary<int, ShapeComponentIds> geometries = new Dictionary<int, ShapeComponentIds>();

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
                gltf.Mesh targetMesh = null;
                foreach (var shapeInstance in shapeInstances.OrderBy(x => x.IfcProductLabel))
                {
                    // we start with a shape instance and then load its geometry.
                    
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
                        targetMesh = new gltf.Mesh
                        {
                            Name = $"Instance {productLabel}"
                        };

                        // link node to mesh
                        tnode.Mesh = meshIndex;
                    }
                    
                    // now the geometry
                    //


                    IXbimShapeGeometryData shapeGeom = geomReader.ShapeGeometry(shapeInstance.ShapeGeometryLabel);
                    if (shapeGeom.Format != (byte)XbimGeometryType.PolyhedronBinary)
                        continue;

                    // work out colour id; 
                    // the colour is associated with the instance, not the geometry.
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

                    if (shapeGeom.ReferenceCount > 1)
                    {
                        // retain the information to reuse the map multiple times
                        //
                        
                        // if g is not found in the dictionary then build it and add it
                        ShapeComponentIds osgGeom;
                        if (!geometries.TryGetValue(shapeGeom.ShapeLabel, out osgGeom))
                        {
                            // mesh 
                            var xbimMesher = new XbimMesher();
                            xbimMesher.AddMesh(shapeGeom.ShapeData);

                            osgGeom = AddGeom(
                                xbimMesher.PositionsAsSingleList(model.ModelFactors.OneMeter),
                                xbimMesher.Indices,
                                xbimMesher.NormalsAsSingleList()
                                );
                            geometries.Add(shapeGeom.ShapeLabel, osgGeom);
                        }

                        if (osgGeom != null)
                        {
                            var arr = GetTransformInMeters(model, shapeInstance);
                            AddComponentsToMesh(targetMesh, osgGeom, materialIndex);
                        }
                    }
                    else
                    {
                        // todo: resume from here!!!
                        
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

        private void AddComponentsToMesh(gltf.Mesh targetMesh, ShapeComponentIds osgGeom, int materialIndex)
        {
            gltf.MeshPrimitive thisPrimitive = new gltf.MeshPrimitive();
            Dictionary<string, int> att = new Dictionary<string, int>();
            att.Add("NORMAL", osgGeom.NormalsAccessorId);
            att.Add("POSITION", osgGeom.VerticesAccessorId);
            thisPrimitive.Attributes = att;
            thisPrimitive.Indices = osgGeom.IndicesAccessorId;
            thisPrimitive.Material = materialIndex;
            thisPrimitive.Mode = gltf.MeshPrimitive.ModeEnum.TRIANGLES;


            int initSize = targetMesh.Primitives.Length;
            if (initSize == 0)
            {
                targetMesh.Primitives = new gltf.MeshPrimitive[] { thisPrimitive };
            }
            else
            {
                var concat = targetMesh.Primitives.ToList();
                concat.Add(thisPrimitive);
                targetMesh.Primitives = concat.ToArray();
            }
        }

        private ShapeComponentIds AddGeom(List<float> positions, List<int> indices, List<float> normals)
        {
            // indices
            ShapeComponentIds ret = new ShapeComponentIds();
            ret.IndicesAccessorId = AddIndices(indices, normals.Count);
            ret.NormalsAccessorId = AddCoordinates(normals);
            ret.VerticesAccessorId= AddCoordinates(positions);
            return ret;
        }

        private int AddCoordinates(List<float> values)
        {
            // buffer preparation
            int startingBufferPoisition = _coordinatesBuffer.Count;
            _coordinatesBuffer.Capacity = startingBufferPoisition + values.Count * sizeof(float);

            
            // prepare to evaluate min max:
            float[] min = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
            float[] max = new float[] { float.MinValue, float.MinValue, float.MinValue };
            

            int i = 0;
            foreach (var value in values)
            {
                // evaluate min/max
                if (value < min[i])
                    min[i] = value;
                if (value > max[i])
                    max[i] = value;
                i++;
                if (i > 2)
                    i = 0;

                // populate the buffer (previously expanded)
                _coordinatesBuffer.AddRange(BitConverter.GetBytes(value));
            }

            var coordAccessor = new gltf.Accessor()
            {
                BufferView = 1,
                ByteOffset = startingBufferPoisition,
                ComponentType = gltf.Accessor.ComponentTypeEnum.FLOAT,
                Normalized = false,
                Count = values.Count / 3,
                Type = gltf.Accessor.TypeEnum.VEC3,
                Min = min,
                Max = max
            };

            // index to return
            var ret = _accessors.Count;
            _accessors.Add(coordAccessor);
            return ret;
        }

        private int AddIndices(List<int> indices, int elementsCount)
        {
            // evaulate min max
            //
            var MinMaxV = indices.Aggregate(new
            {
                MinV = int.MaxValue,
                MaxV = int.MinValue
            },
                (accumulator, o) => new
                {
                    MinV = Math.Min(o, accumulator.MinV),
                    MaxV = Math.Max(o, accumulator.MaxV)
                });

            var indAccessor = new gltf.Accessor
            {
                BufferView = 0,
                ByteOffset = _indicesBuffer.Count,
                Normalized = false,
                Type = gltf.Accessor.TypeEnum.SCALAR,
                Count = indices.Count,
                Min = new float[] { MinMaxV.MinV },
                Max = new float[] { MinMaxV.MaxV }
            };

            // depending on the count of positions and normals, we determine the index type
            // 
            Func<int, byte[]> ToBits;
            var size = 0;
            if (elementsCount <= Math.Pow(2, 8))
            {
                indAccessor.ComponentType = gltf.Accessor.ComponentTypeEnum.UNSIGNED_BYTE;
                size = sizeof(byte);
                ToBits = x => BitConverter.GetBytes((byte)x);
            }
            else if (elementsCount <= Math.Pow(2, 16))
            {
                indAccessor.ComponentType = gltf.Accessor.ComponentTypeEnum.UNSIGNED_SHORT;
                size = sizeof(short);
                ToBits = x => BitConverter.GetBytes((short)x);
            }
            else
            {
                indAccessor.ComponentType = gltf.Accessor.ComponentTypeEnum.UNSIGNED_INT;
                size = sizeof(int);
                ToBits = x => BitConverter.GetBytes((int)x);
            }
            var IndexSize = indices.Count * size;
            List<byte> indicesBufferData = new List<byte>(IndexSize);
            foreach (var index in indices)
            {
                indicesBufferData.AddRange(ToBits(index));
            }
            _indicesBuffer.AddRange(indicesBufferData);
            var thisIndex = _accessors.Count;
            _accessors.Add(indAccessor);
            return thisIndex;
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
