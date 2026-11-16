using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class searchWithFilters : MonoBehaviour
{
    public TMP_InputField municipio;
    public TMP_InputField arma;
    public TMP_InputField ataque;
    public TextMeshProUGUI TMunicipio;
    public TextMeshProUGUI TArma;
    public TextMeshProUGUI TAtaque;
    public TextMeshProUGUI TID;
    public TextMeshProUGUI Tnumper;
    public RawImage image;
    public database scriptables;
    private List<scriptable> scrs;
    private int index;

    void Start()
    {
        scrs = new List<scriptable>();
        index = 0;
    }

    // This is for search in the data base, normally this is the firs search
    private void ForDatabase(string busqueda, int i) 
    {
        foreach (scriptable item in scriptables.allProfiles)
        {
            switch (i) 
            {
                case 1:
                    if (item.municipio == busqueda) 
                    {   
                        scrs.Add(item);
                    }
                    break;

                case 2:
                    if (item.arma == busqueda)
                    {
                        scrs.Add(item);
                    }
                    break;

                case 3:
                    if (item.ataque == busqueda)
                    {
                        scrs.Add(item);
                    }
                    break;
            }
        }
    }

    // This is to remove the items that dont have the correct value
    private void ForArray(string busqueda, int i) 
    {
        foreach (scriptable item in scrs)
        {
            switch (i) 
            {
                case 2:
                    if (item.arma != busqueda) 
                    {
                        scrs.Remove(item);
                    }
                    break; 
                case 3:
                    if (item.ataque != busqueda) 
                    {
                        scrs.Remove(item);
                    }
                    break;
            }
        }
    }
    
    // Show the images in his respectives RawImage
    private void ShowImage() 
    {
        image.texture = null;
        if (scrs[index].image.Length != 0) 
        {
            image.texture = scrs[index].image[0];
        }
        
    }

    // Show the text in his respectives textmeshpro
    private void ShowTexts() 
    {
        TID.text = scrs[index].itemId;
        TMunicipio.text = scrs[index].municipio;
        TArma.text = scrs[index].arma;
        TAtaque.text = scrs[index].ataque;
        Tnumper.text = numPersonas(index,scrs.Count);
    }
    public string numPersonas(int index,int tot)
    {
        int indexShowed = index + 1;
        string numPersons = "Resultados obtenidos: "+indexShowed.ToString() + "/" + tot;
        return numPersons;
    }

    // Find a profile with the values of 3 inputs (Municipio, Arma, Ataque)
    // In this method we have a List that we are adding or removing items depending of the input values
    public void Search() 
    {
        scrs.Clear();
        int i = 0;
        if (!string.IsNullOrEmpty(municipio.text) || !string.IsNullOrEmpty(arma.text) || !string.IsNullOrEmpty(ataque.text)) 
        {
            if (!string.IsNullOrEmpty(municipio.text)) 
            {
                ForDatabase(municipio.text, 1);
                i = 1;
            }
            if (!string.IsNullOrEmpty(arma.text)) 
            {
                if (i == 1)
                {
                    ForArray(arma.text, 2);
                }
                else if(i == 0)
                {
                    ForDatabase(arma.text, 2);
                    i = 1;
                }
            }
            if (!string.IsNullOrEmpty(ataque.text))
            {
                if (i == 0)
                {
                    ForDatabase(ataque.text, 3);
                    i = 1;
                }
                else
                {
                    ForArray(ataque.text, 3);
                }
            }
        }
        index = 0;
        ShowImage();     //Metodo mostrar
        ShowTexts();
    }

    // For a button to move into array
    public void Anterior() 
    {
        if (index > 0) 
        {
            index--;
        }
        ShowImage();
        ShowTexts();
    }

    // For a button to move into array
    public void Siguiente() 
    {
        if (index < scrs.Count-1) 
        {
            index++;
        }
        ShowImage();
        ShowTexts();
    }
}
