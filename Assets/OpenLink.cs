using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLink : MonoBehaviour
{
    

    //open the link to go to gmail sign in
    public void OpenUrl(string enlace)
    {
        Application.OpenURL(enlace);
    }
}
