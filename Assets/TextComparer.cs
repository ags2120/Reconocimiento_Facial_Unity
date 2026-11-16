using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextComparer : MonoBehaviour
{

    //este script se usaria luego para comparar entre la descripcion del paerfil resultado del reconocimiento facial y el texto introducido por el usuario para sacar si wl M.O es lo mismo

    public scriptable scriptableObjectData;
    public TMP_InputField userInputField;
    public TMP_Text similaritiesText;
    private FileBrowserUpdate file;

    void Start() 
    {
        file = GetComponent<FileBrowserUpdate>();
    }

    public void OnUserInputFieldChanged()
    {
        scriptableObjectData.DivideInputFieldText();
    }

    public void DescriptionInput() 
    {
        if (file != null && file.sim != null)
        {
            scriptableObjectData = file.sim;

        }
    }

    // Compare two texts description 
    public void CompareTexts()
    {
        DescriptionInput();
        if (scriptableObjectData != null) 
        {
            OnUserInputFieldChanged();
            string userText = userInputField.text;


            // Split the user input into individual strings using a delimiter (e.g., space)
            string[] userStrings = userText.Split(' ');

            // Compare each user string with each variable text in the scriptable object
            List<string> similarities = new List<string>();
            foreach (string userString in userStrings)
            {
                foreach (string variableText in scriptableObjectData.variableTexts)
                {
                    if (CompareStrings(userString, variableText))
                    {
                        similarities.Add(variableText);

                    }
                }
            }

            // Display the similarities to the user
            DisplaySimilarities(similarities);
        }
    }

    private bool CompareStrings(string text1, string text2)
    {
        // Compare the strings and return true if they are similar, false otherwise
        // You can use any text comparison algorithm or method here based on your requirements

        // For simplicity, this example uses the built-in String.Equals() method for comparison
        return string.Equals(text1, text2);
    }

    private void DisplaySimilarities(List<string> similarities)
    {
        // Generate a formatted string with all the similarities
        string similaritiesString = "Similarities found:\n";
        foreach (string similarity in similarities)
        {
              similaritiesString += "- " + similarity + "\n";
            
        }

        // Display the similarities in the UI text component
        similaritiesText.text = similaritiesString;
        Debug.Log(similaritiesText.text);
    }
}
