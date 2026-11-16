using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataBase", menuName = "DataBases")]
public class database : ScriptableObject
{
    public List<scriptable> allProfiles = new List<scriptable>();// asi podemos coger todos los perfiles creados por el script "scriptable"

   

}
