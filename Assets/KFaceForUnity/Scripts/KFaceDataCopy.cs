using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Kignis;
using System.Runtime.InteropServices;
using System.Linq;

/// <summary>
/// Because Application.StreamingAssetsPath directory is not readable/writeable on all platform, 
/// so here, faces library ,face test images, model files are copied to the Application.The persistentDataPath directory
/// Also, because the directory is not hierarchical, 
/// BuildFileSystemTree is used to get the directory structure information described in StreamingAssets.XML
/// </summary>
public class KFaceDataCopy : MonoBehaviour
{
    int coroutine = 0;
    Node root;

    void Start()
    {
        root = new Node(".");

        TextAsset textAsset = (TextAsset)Resources.Load("StreamingAssets");
        root.XMLStrToNode(textAsset.text);

        root.path = Application.streamingAssetsPath;
        root.BuildPath();

        string srcDir = "Models";
        string dstDir = Path.Combine(Application.persistentDataPath, "Models");

        StartCoroutine(DirFilesCopy(srcDir, dstDir));

        srcDir = "FaceTestLib";
        dstDir = Path.Combine(Application.persistentDataPath, "FaceTestLib");

        StartCoroutine(DirFilesCopy(srcDir, dstDir));

        srcDir = "FaceLib";
        dstDir = Path.Combine(Application.persistentDataPath, "FaceLib");
        if (!Directory.Exists(dstDir)) {
            Directory.CreateDirectory(dstDir);
        }

        Node[] dirNodes = root.GetChild(srcDir).GetDirectoryNodes();

        foreach (Node dir in dirNodes) {
            string srcDirPath = "FaceLib" + "/" + dir.filename;

            string dstDirPath = (new string[] { Application.persistentDataPath, "FaceLib", dir.filename }).Aggregate(Path.Combine);
            StartCoroutine(DirFilesCopy(srcDirPath, dstDirPath));
        }
    }

    void Update()
    {
        //if (coroutine == 0) {
            //When the data is copied, switch scene
            //SceneManager.LoadScene("KFaceExample");
            //coroutine = -1;
        //}
    }

    /// <summary>
    /// Copy files from srcDir to dstDir
    /// </summary>
    /// <returns></returns>
    /// <param name="srcDir">Source directory</param>
    /// <param name="dstDir">Dst directory</param>
    IEnumerator DirFilesCopy(string srcDir, string dstDir)
    {
        coroutine++;
        Node node = root.GetChild(srcDir);
        Node[] files = node.GetFileNodes();

        if (!Directory.Exists(dstDir)) {
            Directory.CreateDirectory(dstDir);
        }

        foreach (Node file in files) {

            string dstPath = Path.Combine(dstDir, file.filename);
            FileInfo dstInfo = new FileInfo(dstPath);

            if (!File.Exists(dstPath) || dstInfo.Length == 0) {
#if UNITY_ANDROID
                WWW www = new WWW(file.path);
#else
                WWW www = new WWW("file://" + file.path);
#endif
                yield return www;
                File.WriteAllBytes(dstPath, www.bytes);
                www.Dispose();
            }
        }
        coroutine--;
    }
}
