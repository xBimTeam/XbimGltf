using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xbim.XbimGLTF.IO
{
    public class Scene
    {
        public List<int> nodes { get; set; }
    }

    public class Node
    {
        public int mesh { get; set; }
    }

    public class Attributes
    {
        public int POSITION { get; set; }
    }

    public class Primitive
    {
        public Attributes attributes { get; set; }
        public int indices { get; set; }
    }

    public class Mesh
    {
        public List<Primitive> primitives { get; set; }
    }

    public partial class Buffer
    {
        private byte[] _bytes;

        public Buffer(byte[] value)
        {
            _bytes = value;
        }

        public string uri {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("data:application/octet-stream;base64,");
                sb.Append(Convert.ToBase64String(_bytes));
                sb.Append("=");
                return sb.ToString();
            }
        }
        public int byteLength
        {
            get
            {
                return _bytes.Length;
            }

        }
    }

    public class BufferView
    {
        public int buffer { get; set; }
        public int byteOffset { get; set; }
        public int byteLength { get; set; }
        public int target { get; set; }
    }

    public class Accessor
    {
        public int bufferView { get; set; }
        public int byteOffset { get; set; }
        public int componentType { get; set; }
        public int count { get; set; }
        public string type { get; set; }
        public List<double> max { get; set; }
        public List<double> min { get; set; }
    }

    public class Asset
    {
        public string version { get; set; } = "2.0";
    }

    public class Gltf2Schema
    {
        public List<Scene> scenes { get; set; }
        public List<Node> nodes { get; set; }
        public List<Mesh> meshes { get; set; }
        public List<Buffer> buffers { get; set; } = new List<Buffer>();
        public List<BufferView> bufferViews { get; set; }
        public List<Accessor> accessors { get; set; }
        public Asset asset { get; set; } = new Asset(); // the default value of 2
    }
}