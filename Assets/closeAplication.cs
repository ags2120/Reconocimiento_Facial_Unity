using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class closeAplication : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(System.IntPtr hwnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    private const int SW_MINIMIZE = 6;

    [DllImport("__Internal")]
    private static extern void UnityMinimizeWindow();

    //Close the aplication
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    //This functions, depending of SO minimize the aplication
    public void Minimize() 
    {
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer)
        {
            // Código específico para Windows
            Debug.Log("Se está ejecutando en Windows");

            System.IntPtr windowHandle = GetActiveWindow();
            ShowWindow(windowHandle, SW_MINIMIZE);

        }
        else if (Application.platform == RuntimePlatform.OSXEditor ||
                 Application.platform == RuntimePlatform.OSXPlayer)
        {
            // Código específico para macOS
            Debug.Log("Se está ejecutando en macOS");

            UnityMinimizeWindow();

        }
    }
}
