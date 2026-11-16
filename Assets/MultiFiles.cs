
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
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
//using System;



public class MultiFiles : MonoBehaviour
{
    KFaceForUnity kface;
    public Texture[] rawImageArr;
    private string _pathFile;
    private List<Texture> rawImage;
    public RawImage imgShow;
    public string[] selectedFiles;


    void Start()
    {
        kface = new KFaceForUnity();
        rawImage = new List<Texture>();


    }
    //Return the image Textures array
    public Texture[] getArray()
    {
        return rawImageArr;
    }

    //Open a filebrowser who can pick one or more images to save in the array
    public void OpenMulti()
    {
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true; // Habilita la selección múltiple de archivos
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"; // Restringe a archivos de imagen
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                selectedFiles = openFileDialog.FileNames;                          
                StartCoroutine(LoadImage(selectedFiles));
    
            }
           
           
        }
       
    }
    //Load all the images selected
    IEnumerator LoadImage(string[] path)
    {

        
        for (int i = 0; i < path.Length; i++)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path[i]))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    var uwrTexture = DownloadHandlerTexture.GetContent(uwr);
                    rawImage.Add(uwrTexture);                  

                }
                if (!string.IsNullOrEmpty(path[i]))
                {
                    _pathFile = path[i];
                }
            }
        }
        rawImageArr = rawImage.ToArray();
        rawImage = new List<Texture>();

    }

}
