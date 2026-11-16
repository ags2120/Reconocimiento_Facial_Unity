using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using UnityEngine.Networking;

public class anotherscript : MonoBehaviour
{
    public string inputFieldValue;
    public string descripcion;
    public TMP_InputField inputField;
    public TMP_InputField inputDescripcion;
    public TMP_InputField inputMunicipio;
    public TMP_InputField inputTipoArma;
    public TMP_InputField inputTipoAtaque;
    public database scriptables;
    private MultiFiles mf;
    public Texture[] rawImageTextures;
    public GameObject gameObj;
    private List<Texture> rawImage;




    /*
     

    private void Start()
    {
        PrimerScript primerScript = GetComponent<PrimerScript>();

        // Suscribirse al evento OnRowImagesReady del PrimerScript
        primerScript.OnRowImagesReady += ReceiveRowImages;
    }

    private void ReceiveRowImages(RawImage[] rowImages)
    {
        // Guardar las imágenes recibidas en el segundo script
        receivedRowImages = rowImages;

        // Realizar cualquier otra operación con las imágenes recibidas
        // ...
    }
}
     */
    public static scriptable modifiedCopy;
    public static datosScriptable dt;

    void Awake()
    {
        LoadData(); // call LoadData
    }
    //There are 2 objects datosScriptable and scriptable, datosScriptable is for save the data in directories and scriptable for the list during de execution.


    // This function takes an array of path images and convert it in textures
    public Texture[] LoadImage(string[] path)
    {
        Texture2D[] texturas = new Texture2D[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            // Leer los bytes del archivo de imagen
            byte[] bytesImagen = File.ReadAllBytes(path[i]);

            // Crear una nueva textura y cargar la imagen desde los bytes
            Texture2D textura = new Texture2D(2, 2);
            textura.LoadImage(bytesImagen);

            // Asignar la textura al array de texturas
            texturas[i] = textura;
        }
        return texturas;

    }

    // This function copy the data of a datosScriptable in the modifiedcopy
    public void CopyDataSCR(datosScriptable ds)
    {
        //Debug.Log("EEEEqqqqqq");
        modifiedCopy = ScriptableObject.CreateInstance<scriptable>();

        rawImageTextures = LoadImage(ds.image);
        modifiedCopy.image = rawImageTextures;

        modifiedCopy.itemId = ds.itemId;
        modifiedCopy.descripcion = ds.descripcion;

        modifiedCopy.municipio = ds.municipio;
        modifiedCopy.arma = ds.arma;
        modifiedCopy.ataque = ds.ataque;


        scriptables.allProfiles.Add(modifiedCopy);
    }

    public void CopyImage(string sourcePath, string destinationPath)
    {
        File.Copy(sourcePath, destinationPath, true);
    }
    // This function copy the modifiedcopy in a datosScriptable
    public void CopyData(string carpeta)
    {
        dt = new datosScriptable();
        
        dt.itemId = modifiedCopy.itemId;
        dt.descripcion = modifiedCopy.descripcion;
        dt.municipio = modifiedCopy.municipio;
        dt.arma = modifiedCopy.arma;
        dt.ataque = modifiedCopy.ataque;

        gameObj = GameObject.Find("control");
        MultiFiles mf = gameObj.GetComponent<MultiFiles>();
        string[] str = new string[mf.selectedFiles.Length];
        for (int i = 0; i < mf.selectedFiles.Length; i++)
        {
            CopyImage(mf.selectedFiles[i], carpeta + "/" + dt.itemId + "/" + dt.itemId + i.ToString() + ".png");
            str[i] = carpeta + "/" + dt.itemId + "/" + dt.itemId + i.ToString() + ".png";
        }
        dt.image = str;
    }
    // This function take the data of the json and put it in a item of the list
    public void LoadItem(string ruta) 
    {
        if (File.Exists(ruta)) 
        {
            string textJson = File.ReadAllText(ruta);
            var scr = JsonUtility.FromJson<datosScriptable>(textJson);
            //Debug.Log(scr.image);
            CopyDataSCR(scr);
            //Debug.Log(scr.descripcion);
        }
    }

    // This function save the data of the directories of persistentDataPath/Perfiles in the List of scriptableObjects
    public void LoadData() 
    {
        
        string carpeta = Application.persistentDataPath + "/Perfiles";

        string[] archivos = Directory.GetDirectories(carpeta);

        int numeroArchivos = archivos.Length;

        for (int i = 0; i < numeroArchivos; i++) 
        {
            string[] json = Directory.GetFiles(archivos[i], "*.json");
            LoadItem(json[0]);
        }
    }
    // This function save a profile like a directory and a json with his images in persistentDataPath/Perfiles
    public void GuardarJSON() 
    {
        string carpeta = Application.persistentDataPath + "/Perfiles";
        bool checkFolder = Directory.Exists(carpeta);
        if (checkFolder == false)
        {
            Directory.CreateDirectory(carpeta);
        }
        string fullPath = carpeta + "/" + modifiedCopy.itemId + "/";
        string path = carpeta + "/" + modifiedCopy.itemId + "/" + modifiedCopy.itemId + ".json";
        
        Directory.CreateDirectory(fullPath);


        CopyData(carpeta);

        string json = JsonUtility.ToJson(dt);
        File.WriteAllText(path, json);
        Debug.Log(json);
    }

    public string[] CopyStringArray(string[] sourceArray)
    {
        string[] copy = new string[sourceArray.Length];

        // Copy each element from the source array to the destination array
        for (int i = 0; i < sourceArray.Length; i++)
        {
            copy[i] = sourceArray[i];

        }
        return copy;
    }

    public void CopyTextureArray(Texture[] sourceArray)
    {
        // Debug.Log("Entro a copyTexture");
        // Create a new array with the same length as the source array
        rawImageTextures = null;
        rawImageTextures = new Texture[sourceArray.Length];

        // Copy each element from the source array to the destination array
        for (int i = 0; i < sourceArray.Length; i++)
        {
            rawImageTextures[i] = sourceArray[i];

        }
    }
    
    // Add a scriptable to the database 
    public void AddCopyToList()
    {

        modifiedCopy = ScriptableObject.CreateInstance<scriptable>();
        // scriptable modifiedCopy = ScriptableObject.CreateInstance<scriptable>();
        //   FileBrowserUpdate img;

        inputFieldValue = inputField.text;//this is for the id
        descripcion = inputDescripcion.text;//this is for the description
                                            // Create a new instance of the ScriptableObject

        //CopyTextureArray(rawImageTextures);
        //modifiedCopy.image = fileBrowser.loadImage(selectedFiles);
        //Debug.Log("array vacío");
        /*if (modifiedCopy.image == null)
        {
            Debug.Log("array vacío");
        }*/
        //modifiedCopy.image = new Texture2D[selectedFiles.Length];
        /*if(mf == null )
        {
            Debug.Log("mf vacio");
        }*/
        //rawImageTextures = mf.rawImageArr;
        //modifiedCopy.image = rawImageTextures;
        // Copy the values from the original ScriptableObject to the modified copy
        // modifiedCopy.altura = originalscriptableobject.altura;

        //modifiedCopy.image = rawImageTextures;

        /*foreach(Texture s in rawImageTextures)
        {
            foreach (Texture t in mf.rawImageArr)
            {
                s = t;
            }
        }*/

        /* if (modifiedCopy.image.Length == 0)
         {
             Debug.Log("modifiedCopy == 0");
         }*/
        gameObj = GameObject.Find("control");
        MultiFiles mf = gameObj.GetComponent<MultiFiles>();
        CopyTextureArray(mf.rawImageArr);

        modifiedCopy.image = rawImageTextures;
        modifiedCopy.itemId = inputFieldValue;
      //  modifiedCopy.image = img.rawImage.texture;
        modifiedCopy.descripcion = descripcion;

        modifiedCopy.municipio = inputMunicipio.text;
        modifiedCopy.arma = inputTipoArma.text;
        modifiedCopy.ataque = inputTipoAtaque.text;
     //   t.itemId = inputFieldValue;
        // Modify the variable of the modified copy
       // modifiedCopy.altura = 42;
        Debug.Log(modifiedCopy.itemId);

        // Add the modified ScriptableObject to the list
          scriptables.allProfiles.Add(modifiedCopy);

        GuardarJSON();

       //scriptableObjectList.Add(modifiedCopy);
    }

}
