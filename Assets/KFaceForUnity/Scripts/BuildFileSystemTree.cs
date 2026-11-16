using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Kignis;

namespace Kignis
{
	/// <summary>
	/// Node represents a file or directory node
	/// </summary>
    public class Node
    {
        public List<Node> children;
        public Node parent;
        public string filename;
        public string path;

        public Node(string name)
        {
            children = new List<Node>();
            filename = name;
            path = name;
        }

        public Node()
        {
            children = new List<Node>();
            filename = "";
            path = "";
        }

        public void AddNode(Node node)
        {
            children.Add(node);
        }
			
        public void BuildPath()
        {
            foreach (Node node in children) {
                node.path = path + "/" + node.filename;
                if (node.children.Count > 0) {
                    node.BuildPath();
                }
            }
        }

        public string[] GetFileNames()
        {
            List<string> filesName = new List<string>();

            foreach (Node node in children) {
                if (node.children.Count == 0) {
                    filesName.Add(node.filename);
                }
            }
            return filesName.ToArray();
        }

        public string[] GetDirectoryNames()
        {
            List<string> filesName = new List<string>();

            foreach (Node node in children) {
                if (node.children.Count > 0) {
                    filesName.Add(node.filename);
                }
            }
            return filesName.ToArray();
        }

        public Node[] GetFileNodes()
        {
            List<Node> fileNode = new List<Node>();

            foreach (Node node in children) {
                if (node.children.Count == 0) {
                    fileNode.Add(node);
                }
            }
            return fileNode.ToArray();
        }

        public Node[] GetDirectoryNodes()
        {
            List<Node> dirNode = new List<Node>();

            foreach (Node node in children) {
                if (node.children.Count > 0) {
                    dirNode.Add(node);
                }
            }
            return dirNode.ToArray();
        }

        public Node GetChild(string filepath)
        {
            string[] paths = filepath.Split('/');
            Node index = this;
            bool pathErr = false;

            for (int i = 0; i < paths.Length; i++) {
                if (paths[i] == "")
                    continue;

                bool found = false;
                foreach (Node node in index.children) {
                    if (node.filename == paths[i]) {
                        index = node;
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    pathErr = true;
                    break;
                }
            }
            if (!pathErr)
                return index;
            return null;
        }

        public void NodeToXMLFile(string filename)
        {
            TextWriter fileWriter = new StreamWriter(filename);
            XmlSerializer serializer = new XmlSerializer(typeof(Node));
            serializer.Serialize(fileWriter, this);
            fileWriter.Close();
        }

        public string NodeToXMLStr()
        {
            StringWriter stringWriter = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(Node));
            serializer.Serialize(stringWriter, this);
            return stringWriter.ToString();
        }

        public void XMLFileToNode(string filename)
        {
            Node node;
            TextReader fileReader = new StreamReader(filename);
            XmlSerializer serializer = new XmlSerializer(typeof(Node));
            node = (Node)serializer.Deserialize(fileReader);
            fileReader.Close();
            children = node.children;
            filename = node.filename;
        }

        public void XMLStrToNode(string xml)
        {
            Node node;
            StringReader stringReader = new StringReader(xml);
            XmlSerializer serializer = new XmlSerializer(typeof(Node));
            node = (Node)serializer.Deserialize(stringReader);
            children = node.children;
            filename = node.filename;
        }
    }
}

/// <summary>
/// Build a tree of file systems described using XML, the root is Application.streamingAssetsPath
/// </summary>
public class BuildFileSystemTree : MonoBehaviour
{
    Node root;
    void Start()
    {
        root = new Node(".");

        //Models
        string srcDir = System.IO.Path.Combine(Application.streamingAssetsPath, "Models");
        DirectoryInfo dinfo = new DirectoryInfo(srcDir);
        FileInfo[] files = dinfo.GetFiles();

        Node modelsDir = new Node("Models");

        foreach (FileInfo f in files) {
            if (f.Extension == ".bin" || f.Extension == ".param")
                modelsDir.AddNode(new Node(f.Name));
        }
        root.AddNode(modelsDir);

        //FaceTestLib
        srcDir = System.IO.Path.Combine(Application.streamingAssetsPath, "FaceTestLib");
        dinfo = new DirectoryInfo(srcDir);
        files = dinfo.GetFiles();

        Node faceTestDir = new Node("FaceTestLib");

        foreach (FileInfo f in files) {
            if (f.Extension == ".jpg" || f.Extension == ".png")
                faceTestDir.AddNode(new Node(f.Name));
        }

        root.AddNode(faceTestDir);

        //FaceLib
        srcDir = System.IO.Path.Combine(Application.streamingAssetsPath, "FaceLib");
        dinfo = new DirectoryInfo(srcDir);
        DirectoryInfo[] dirsInfo = dinfo.GetDirectories();

        Node faceLibDir = new Node("FaceLib");

        foreach (DirectoryInfo dir in dirsInfo) {
            Node samePersonFacesNode = new Node(dir.Name);

            foreach (FileInfo f in dir.GetFiles()) {
                if (f.Extension == ".jpg" || f.Extension == ".png" || f.Extension == ".xml")
                    samePersonFacesNode.AddNode(new Node(f.Name));
            }
            faceLibDir.AddNode(samePersonFacesNode);
        }

        root.AddNode(faceLibDir);
        root.BuildPath();
        root.NodeToXMLFile(System.IO.Path.Combine(Application.streamingAssetsPath, "StreamingAssets.xml"));
    }
}
