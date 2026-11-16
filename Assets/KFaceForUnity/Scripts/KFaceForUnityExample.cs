using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Kignis;

/// <summary>
/// Face lib search result
/// </summary>
public struct FaceLibSearchResult
{
    public string personID;
    public string filename;
	// Cosine similarity(from -1.0 to 1.0)
    public double sim;
}

/// <summary>
/// This example shows how to find the most similar person from the face database
/// The face library contains only 50 people, each of whom has 8 photos at most, 
/// all of which are from the CASIA-webface face library
/// Full face library download address: http://www.cbsr.ia.ac.cn/english/CASIA-WebFace-Database.html
/// The corresponding names are stored in Resource/names.txt
/// </summary>
public class KFaceForUnityExample : MonoBehaviour
{
    KFaceForUnity kface;
    Dictionary<string, Dictionary<string, MobileFaceNetFeatures>> faceLibFeatures;

    public RawImage testImage;
	// only show 4 pictures
    public RawImage[] libImages;
    public Text nameText;
    public Text timeText;

    List<FileInfo> testImages;
    int testImagesIndex = 0;

    Texture unknownLibImg;
    Dictionary<string, string> IDNameMap;

    void Start()
    {
        unknownLibImg = Resources.Load<Texture>("Textures/unknownLibImg");
        TextAsset names = Resources.Load<TextAsset>("names");

        IDNameMap = new Dictionary<string, string>();
        ReadNames(names);
    
        kface = new KFaceForUnity();
        
		// init the model 
        bool ret = kface.InitModel(Path.Combine(Application.persistentDataPath, "Models"));
        Debug.Log("KFaceForUnity InitModel:" + ret);

        testImages = new List<FileInfo>();
        DirectoryInfo dir = new DirectoryInfo(Path.Combine(Application.persistentDataPath, "FaceLib"));
        DirectoryInfo[] dirs = dir.GetDirectories();

        faceLibFeatures = new Dictionary<string, Dictionary<string, MobileFaceNetFeatures>>();

        foreach (DirectoryInfo d in dirs) {
            string personID = d.Name;
            FileInfo[] featuresFile = d.GetFiles();

            Dictionary<string, MobileFaceNetFeatures> samePersonFaceFeatures = new Dictionary<string, MobileFaceNetFeatures>();

            foreach (FileInfo f in featuresFile) {
                if (f.Extension == ".xml") {
                    MobileFaceNetFeatures fs;

                    fs = kface.XMLFileToFeatures(f.FullName);
                    samePersonFaceFeatures.Add(f.Name, fs);
                }
            }

            if (samePersonFaceFeatures.Count > 0)
                faceLibFeatures.Add(personID, samePersonFaceFeatures);
        }

        dir = new DirectoryInfo(Path.Combine(Application.persistentDataPath, "FaceTestLib"));

        foreach (FileInfo f in dir.GetFiles()) {
            if (f.Extension == ".jpg" || f.Extension == ".png") {
                testImages.Add(f);
            }
        }

		// Let`s begin with the first
        SearchThePerson(0);
    }
		
	/// <summary>
	/// Pick the next face test image to do the test
	/// </summary>
    public void OnNextClick()
    {
        if (testImagesIndex < testImages.Count - 1) {
            testImagesIndex++;
            SearchThePerson(testImagesIndex);
        }
    }

	/// <summary>
	/// Pick the prev face test image to do the test
	/// </summary>
    public void OnPrevClick()
    {
        if (testImagesIndex > 0) {
            testImagesIndex--;
            SearchThePerson(testImagesIndex);
        }
    }

	/// <summary>
	/// Find the most similar person from the face library with the index person
	/// </summary>
	/// <param name="index">Face test image index</param>
    protected void SearchThePerson(int index)
    {
        bool personFound = false;
        double delta;

        Texture2D tex;
        FaceRecognitionResultWithTex2D frrTex;
        int i = 0;

        tex = kface.ReadAsTex(testImages[index].FullName);

        double begin = Time.realtimeSinceStartup;
		// Get face features, which is actually 128 floating point values storing in MobileFaceNetFeatures struct
        frrTex = kface.GetFeatures(tex, KFaceForUnity.Draw.BOTH, KFaceForUnity.ImageFlip.Vert);

        if (frrTex.frr.netNum > 0) {
            FaceLibSearchResult flsr;

			// Compare it with all the other images in the face library
            flsr = CompareWithLibFaces(frrTex.frr.facenetfs[0]);

            DirectoryInfo dir = new DirectoryInfo((new string[]{Application.persistentDataPath, "FaceLib", flsr.personID}).Aggregate(Path.Combine));
            FileInfo[] files = dir.GetFiles();

			// Face cosine similarity must be greater than 0.4,
			// the larger the value, the more similar
            if (flsr.personID != "" && flsr.sim > 0.4) {
                Texture2D libTex;

                personFound = true;

                if (IDNameMap.ContainsKey(flsr.personID))
                    nameText.text = IDNameMap[flsr.personID];
                else
                    nameText.text = "UnKnown";

                foreach (FileInfo f in files) {
                    if (f.Extension != ".jpg" && f.Extension != ".png")
                        continue;

                    libTex = kface.ReadAsTex(f.FullName);

                    if (i < libImages.Length) {
                        libImages[i].texture = libTex;
                    }
                    i++;
                }
            }
        }

        if (!personFound) {
            nameText.text = "UnKnown";
            for (i = 0; i < libImages.Length; i++) {
                libImages[i].texture = unknownLibImg;
            }
        }

        testImage.texture = frrTex.tex;

        // release the loaded image textures!
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        delta = Time.realtimeSinceStartup - begin;
        timeText.text = delta.ToString("0.00") + " s";
    }

	/// <summary>
	/// Find the most similar person
	/// </summary>
	/// <returns>Search result</returns>
	/// <param name="fs">Face features extracted by the neural network</param>
    FaceLibSearchResult CompareWithLibFaces(MobileFaceNetFeatures fs)
    {
        FaceLibSearchResult flsr;
        double maxSim = -1.0;
        double tmpSim = -1.0;
        string maxSimPersonID = "";
        string maxSimFilename = "";

        foreach (string personID in faceLibFeatures.Keys) {
            foreach (string filename in faceLibFeatures[personID].Keys) {
                tmpSim = kface.GetSimilarity(fs, faceLibFeatures[personID][filename]);

                if (tmpSim > maxSim) {
                    maxSim = tmpSim;
                    maxSimPersonID = personID;
                    maxSimFilename = filename;
                }
            }
        }

        flsr.personID = maxSimPersonID;
        flsr.filename = maxSimFilename;
        flsr.sim = maxSim;

        return flsr;
    }

	/// <summary>
	/// Read the names
	/// </summary>
	/// <param name="names">asset</param>
    void ReadNames(TextAsset names)
    {
        string text = names.text;
        string[] nameArray = text.Split('\n');

        foreach (string str in nameArray) {
            string[] propertyArray = str.Split(' ');
            string personID = propertyArray[0];
            string name = propertyArray[1];

            IDNameMap.Add(personID, name);
        }
    }
}
