using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class oneInstanceDatabase : MonoBehaviour
{
    //a script to check that there is only one data base chosen and that it does exist in the data base
    public database scriptables;
    public static oneInstanceDatabase instance;
    public TMP_Text textID;
    private tester test;

    void Start() 
    {
        test = GetComponent<tester>();
    }

    private void Awake()
    {

        
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }

   
       public static scriptable GetItemById(string id)
    {
       /*if(instance.scriptables.allProfiles == null)
        {

        }*/

        foreach (scriptable item in instance.scriptables.allProfiles)
        {
            //Debug.Log("getItem");
            if (item.itemId == id)
            {
                return item;
            }
           
        }
            return null;
    }

    // Remove a scriptable from the list and the folder of the profile in the persistentDataPath/Perfiles
    public void removeScriptable()//later this will get developed
    {
        if (!string.IsNullOrEmpty(textID.text.ToString())) 
        {

            string carpeta = Application.persistentDataPath + "/Perfiles/" + textID.text.ToString();

            Directory.Delete(carpeta, true);
           
            instance.scriptables.allProfiles.Remove(GetItemById(textID.text));
            test.RemoveImages();
            
        }
        
    }

}
