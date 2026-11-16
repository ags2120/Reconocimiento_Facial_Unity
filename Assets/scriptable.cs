using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kignis;


[CreateAssetMenu(fileName = "perfiles", menuName = "perfil")]

public class scriptable : ScriptableObject
{
     public string itemId;
    //public RawImage image;
    // public Sprite icon;
   // public RawImage rawimage;
    public Texture[] image;

    public double similarity;
    // we use this to take the image directly without changing it to a sprite
    // public int altura;
    public string descripcion;

    public string municipio;
    public string arma;
    public string ataque;

    public string[] variableTexts;
  //  public string inputFieldText;//se esta usando este ahora para la comparacion,lurgo se incluye un texto para la descripcio de que este hizo y se compara con el script"taxtComparer"
   

    public void DivideInputFieldText()
    {
        variableTexts = descripcion.Split(' ');
    }
  
}

