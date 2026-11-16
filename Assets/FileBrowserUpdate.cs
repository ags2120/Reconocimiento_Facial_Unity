
using AnotherFileBrowser.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Kignis;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using TMPro;

public class FileBrowserUpdate : MonoBehaviour
{
    KFaceForUnity kface;
    public RawImage rawImage;
    public RawImage[] similarities;
    private static string _pathFile;
    private static Texture2D tex;
    public Texture clearedImage;
    public database scriptables;
    public TMP_Text textID;
    public TMP_Text descrip;
    public scriptable sim;
    
    // Initialize the library of KfaceForUnity
    void Start() 
    {
        kface = new KFaceForUnity();
        Debug.Log(Path.Combine(UnityEngine.Application.persistentDataPath, "Models"));
        bool ret = kface.InitModel(Path.Combine(UnityEngine.Application.persistentDataPath, "Models"));
        Debug.Log("KFaceForUnity InitModel:" + ret);
        
    }

    // To add a image with the archive explorer 
    public void OpenFileBrowser()
    {
        var bp = new BrowserProperties();
        bp.filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";//later search if you must keep only"png" and "gpj"
        bp.filterIndex = 0;
        

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            //Load image from local path with UWR
            StartCoroutine(LoadImage(path));
        });
    }

    // To take the image
    IEnumerator LoadImage(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                var uwrTexture = DownloadHandlerTexture.GetContent(uwr);
                rawImage.texture = uwrTexture;
                tex = uwrTexture;
                //anotherscript.modifiedCopy.image.texture = uwrTexture;
            }
            if (!string.IsNullOrEmpty(path))
            {
                _pathFile = path;
            }
        }

    }

    // This method is not used but is for take a feature of KFaceForUnity and save it like a XML
    public void SaveImageAsXMLInDatabase()
    {
        FileInfo info = new FileInfo(_pathFile);

        if (!string.IsNullOrEmpty(_pathFile))
        {
            if (info.Extension == ".jpg" || info.Extension == ".png")
            {

                FaceRecognitionResult frr = kface.GetFeatures(_pathFile);
                int i = 0;
                foreach (MobileFaceNetFeatures fs in frr.facenetfs)
                {
                    //kface.FeaturesToXMLFile(fs, @"M:\image\Assets\XMLs\" + Path.GetFileNameWithoutExtension(info.Name) + ".xml");
                    i++;
                }

                //XmlDocument xmlDoc = kface.FeaturesToXMLDocument(fs);
                //Guardar En base de datos

            }
        }
    }

    // For look for a item in the list depending of his itemId
    public scriptable GetItemById(string id)
    {

        foreach (scriptable item in scriptables.allProfiles)
        {
            if (item.itemId == id)
            {
                return item;
            }

        }
        return null;
    }

    // To remove a image after a Compare
    public void RemoveImages()
    {
        for (int i = 0; i < similarities.Length; i++)
        {
            similarities[i].texture = clearedImage;
            //Debug.Log("Borro");
        }
    }

    // Search a person a compare the results
    public void SearchPersonAndCompare() 
    {
        RemoveImages();
        if (tex != null) 
        {
            FaceRecognitionResultWithTex2D frrTex = kface.GetFeatures(tex, KFaceForUnity.Draw.BOTH, KFaceForUnity.ImageFlip.Vert);

            if (frrTex.frr.netNum > 0)
            {
                scriptable scr;

                scr = CompareWithLibFaces(frrTex.frr.facenetfs[0]);

                if (!string.IsNullOrEmpty(scr.itemId) && scr.similarity > 0.4)
                {
                    Debug.Log(scr.similarity);
                    sim = GetItemById(scr.itemId);

                    textID.text = sim.itemId;
                    descrip.text = sim.descripcion;

                    for (int i = 0; i< similarities.Length;i++) 
                    {
                        if (sim.image.Length > i && sim.image[i] != null) 
                        {
                            similarities[i].texture = sim.image[i];
                            Debug.Log(i);
                        }
                        
                    }

                }
                else 
                {
                    Debug.Log("Unknown");
                }
            }
        }
    }

    // This function compare the results
    scriptable CompareWithLibFaces(MobileFaceNetFeatures fs)
    {
        scriptable scr = new scriptable();
        double maxSim = -1.0;
        double tmpSim = -1.0;
        string maxSimPersonID = "";
        string maxSimDescripcion = "";

        foreach (scriptable ID in scriptables.allProfiles)
        {
            foreach (Texture tex1 in ID.image)
            {
                if (tex1 != null) 
                {
                    Texture2D tex2D = new Texture2D(tex1.width, tex1.height, TextureFormat.RGBA32, false);
                    RenderTexture.active = RenderTexture.GetTemporary(tex1.width, tex1.height, 32);
                    Graphics.Blit(tex1, RenderTexture.active);
                    tex2D.ReadPixels(new Rect(0, 0, tex1.width, tex1.height), 0, 0);
                    tex2D.Apply();
                    RenderTexture.active = null;

                    FaceRecognitionResultWithTex2D frrTex = kface.GetFeatures(tex2D, KFaceForUnity.Draw.BOTH, KFaceForUnity.ImageFlip.Vert);
                    tmpSim = kface.GetSimilarity(fs, frrTex.frr.facenetfs[0]);

                    if (tmpSim > maxSim)
                    {
                        maxSim = tmpSim;
                        maxSimPersonID = ID.itemId;
                        maxSimDescripcion = ID.descripcion;
                    }
                }

            }
        }

        scr.similarity = maxSim;
        scr.itemId = maxSimPersonID;
        scr.descripcion = maxSimDescripcion;

        Debug.Log(scr.similarity);
        Debug.Log(scr.itemId);

        return scr;
    }


}

