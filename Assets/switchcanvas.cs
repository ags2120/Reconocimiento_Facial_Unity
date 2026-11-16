using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchcanvas : MonoBehaviour
{
    public Canvas searchingCanvas;
    //public Canvas nuevoPerfil;
    //public Canvas moCanvas;
    public Canvas realTime;
    public GameObject quadObj;
    public GameObject panel;
    private bool cambio = true;
    public Camera camera1;
    public Camera camera2;
    private Canvas canvas_copia;
    public GameObject gameObj;
    private List<GameObject> objs;

    void Awake()
    {
        searchingCanvas.enabled = true;
        //nuevoPerfil.enabled = true;
       // moCanvas.enabled = true;
        realTime.enabled = false;
        quadObj.SetActive(false);
        camera1.gameObject.SetActive(true);
        camera2.gameObject.SetActive(false);
        canvas_copia = realTime;
        canvas_copia.enabled = true;
        panel.SetActive(false);

        // DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        
    }

    //Change between RealTime canvas(for the camera) and the aplication canvas(for the rest of functions)
    public void ChangeCanvas()
    {
        

        if (cambio)
        {
            searchingCanvas.enabled = false;
            //nuevoPerfil.enabled = false;
           // moCanvas.enabled = false; 
            realTime.enabled = true;
            realTime.gameObject.SetActive(true);
            panel.SetActive(true);
            quadObj.SetActive(true);
            camera1.gameObject.SetActive(false);
            camera2.gameObject.SetActive(true);


        }
        else
        {
            searchingCanvas.enabled = true;
           // nuevoPerfil.enabled = true;
           //moCanvas.enabled = true;
            realTime.enabled = false;
            realTime.gameObject.SetActive(false);
            panel.SetActive(false);
            quadObj.SetActive(false);
            camera1.gameObject.SetActive(true);
            camera2.gameObject.SetActive(false);

            /*gameObj = GameObject.Find("Tabs");
            TabGroup sc = gameObj.GetComponent<TabGroup>();
            sc.objectsToSwap[0].SetActive(true);*/


        }
        cambio = !cambio;

    }
}
