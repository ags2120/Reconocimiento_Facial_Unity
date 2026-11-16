using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class tester : MonoBehaviour
{
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    // public Text itemAltura;
    //public RawImage[] itemSprite = new RawImage[4];//we are using this to show the image of each profile on the database
    public List<RawImage> rawImage;
    public RawImage[] itemSprite;
    public Texture clearedImage;
    [Space]
    
    public TMP_InputField inputField;
    public string inputFieldValue;
    
    public void Start()
    {
        rawImage = new List<RawImage>();
    }


    public void getItemById()

    {
        
        inputFieldValue = inputField.text;
        Debug.Log(inputFieldValue);
        setUi(oneInstanceDatabase.GetItemById(inputFieldValue));
    }

    // For remove images after a search
    public void RemoveImages() 
    {
        for (int i = 0; i < itemSprite.Length; i++)
        {
            itemSprite[i].texture = clearedImage;
            Debug.Log("Borro");
        }
    }

    //Shows the result of the sarch a user
    private void setUi(scriptable perfil)
    {
        if(perfil == null)
        {
            itemNameText.text = "error";
            itemDescriptionText.text = "error";
            // itemAltura.text = "error";
            RemoveImages();
        }
        else
        {

            RemoveImages();
            itemNameText.text = perfil.itemId.ToString();
            
           
            itemDescriptionText.text = perfil.descripcion;

            // itemSprite.texture = perfil.image.texture;

            
            for (int i = 0;i< itemSprite.Length;i++)
            {
                if (i < perfil.image.Length)
                {

                    Texture imagenFuente = perfil.image[i];
                    
                    itemSprite[i].texture = imagenFuente;
                   
                }
                

            }
            
            

        }
       
    }
}
