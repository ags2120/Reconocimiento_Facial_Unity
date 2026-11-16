using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Kignis
{
    /// <summary>
    /// KRect is a struct of rectangle to hold the face detect result
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KRect
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }

    /// <summary>
    /// MobileFaceNetFeatures is a struct of face features extracted by CNN neural network(mobilefacenet)
    /// The feature field are 128 floating point numbers
    /// The index field is the index of the KRect of multiple face recognition results
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MobileFaceNetFeatures
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public float[] feature;
        public int index;
    }

    /// <summary>
    /// FaceRecognitionResult is a struct of face recognition result
    /// </summary>
    public struct FaceRecognitionResult
    {
        public int rectNum;
        public int netNum;
        public KRect[] rects;
        public MobileFaceNetFeatures[] facenetfs;
        public KFaceForUnity.ErrCode errCode;
    }

    /// <summary>
    /// FaceRecognitionResultWithTex2D contains a texture
    /// </summary>
    public struct FaceRecognitionResultWithTex2D
    {
        public FaceRecognitionResult frr;
        public Texture2D tex;
    }

    public class KFaceForUnity
    {
        public enum ImageFlip { Vert, Hori, NONE };
        public enum ErrCode { TextureError, ModelError, NONE };

        /// <summary>
        /// Draw landmark or rectangle on the image
        /// </summary>
        public enum Draw { RECT, LANDMARK, BOTH, NONE };

        public const string MODEL_DETECTOR_NAME = "detector.bin";
        public const string MODEL_ALIGNLITE_NAME = "alignLite.bin";
        public const string MODEL_FACENETBIN_NAME = "mobilefacenet.bin";
        public const string MODEL_FACENET_NAME = "mobilefacenet.param";

        [DllImport("KFaceForUnity")]
        private static extern IntPtr init(byte[] detectModel, byte[] alignModel, byte[] modelBin, byte[] modelParams, double minSize, double maxSize, int threadNum);

        [DllImport("KFaceForUnity")]
        private static extern void release(IntPtr pKFace);

        [DllImport("KFaceForUnity")]
        private static extern int detectAndExtract(IntPtr pKFace, byte[] Img, int width, int height, ref IntPtr ppRect, ref int pRectNum, ref IntPtr ppNetFs, ref int pNetNum, Draw drawLandmarks);

        [DllImport("KFaceForUnity")]
        private static extern bool freeDetectAlignNormalizeMemory(ref IntPtr pRect, ref IntPtr pNetFs);

        [DllImport("KFaceForUnity")]
        private static extern void textureFlip(byte[] Img, int width, int height, int doFlip);

        [DllImport("KFaceForUnity")]
        private static extern double computeSimilarity(ref MobileFaceNetFeatures pNetFs1, ref MobileFaceNetFeatures pNetFs2);

        /// <summary>
        /// A pointer get from native library
        /// </summary>
        protected IntPtr _pKFace;

        public KFaceForUnity()
        {
            _pKFace = IntPtr.Zero;
        }

        ~KFaceForUnity()
        {
            if (_pKFace != IntPtr.Zero) {
                release(_pKFace);
                _pKFace = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Initialization model files
        /// </summary>
        /// <param name="modelsDir">Model files path</param>
        /// <param name="minSize">The smallest size of a face relative to an image</param>
        /// <param name="maxSize">The maximum size of a face relative to an image</param>
        /// <param name="threadNum">The number of thread to do neural network</param>
        /// <returns>Whether the initialization was successful</returns>
        public bool InitModel(string modelsDir, double minSize = 0.1, double maxSize = 0.8, int threadNum = 4)
        {
            IntPtr p;

            p = _reloadModel(modelsDir, minSize, maxSize, threadNum);
            if (p != IntPtr.Zero) {
                release(_pKFace);
                _pKFace = p;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Initialization model files
        /// </summary>
        /// <param name="modelsDir">Model files path</param>
        /// <param name="minSize">The smallest size of a face relative to an image</param>
        /// <param name="maxSize">The maximum size of a face relative to an image</param>
        /// <param name="threadNum">The number of thread to do neural network</param>
        /// <returns>Whether the initialization was successful</returns>
        protected IntPtr _reloadModel(string modelsDir, double minSize = 0.1, double maxSize = 0.8, int threadNum = 4)
        {
            byte[] detectFileB;
            byte[] alignFileB;
            byte[] ncnnBinFileB;
            byte[] ncnnParamFileB;
            string filename;

            filename = Path.Combine(modelsDir, MODEL_DETECTOR_NAME);
            if (!File.Exists(filename))
                return IntPtr.Zero;

            detectFileB = Encoding.ASCII.GetBytes(filename + '0');
            detectFileB[detectFileB.Length - 1] = 0;

            filename = Path.Combine(modelsDir, MODEL_ALIGNLITE_NAME);
            if (!File.Exists(filename))
                return IntPtr.Zero;
            alignFileB = Encoding.ASCII.GetBytes(filename + '0');
            alignFileB[alignFileB.Length - 1] = 0;

            filename = Path.Combine(modelsDir, MODEL_FACENETBIN_NAME);
            if (!File.Exists(filename))
                return IntPtr.Zero;

            ncnnBinFileB = Encoding.ASCII.GetBytes(filename + '0');
            ncnnBinFileB[ncnnBinFileB.Length - 1] = 0;

            filename = Path.Combine(modelsDir, MODEL_FACENET_NAME);
            if (!File.Exists(filename))
                return IntPtr.Zero;

            ncnnParamFileB = Encoding.ASCII.GetBytes(filename + '0');
            ncnnParamFileB[ncnnParamFileB.Length - 1] = 0;

            return init(detectFileB, alignFileB, ncnnBinFileB, ncnnParamFileB, minSize, maxSize, threadNum);
        }

        /// <summary>
        /// Do face detection,face alignment,face normalization,face feature extraction for the texture
        /// </summary>
        /// <param name="tex">Process on this texture</param>
        /// <param name="draw">Whether to draw a landmark or rectangle</param>
        /// <param name="flip">How to mirror the image</param>
        /// <returns>face recognition result with texture</returns>
        public FaceRecognitionResultWithTex2D GetFeatures(Texture2D tex, Draw draw = Draw.BOTH, ImageFlip flip = ImageFlip.Vert)
        {
            FaceRecognitionResultWithTex2D frrTex;

            if (tex == null)
                return BuildErrorResultWithTex2D(ErrCode.TextureError);

            byte[] imgData = Color32ArrayToByteArray(tex.GetPixels32());

            frrTex.frr = GetFeatures(imgData, tex.width, tex.height, draw, flip);
            frrTex.tex = null;

            if (draw != Draw.NONE) {
                if (flip == ImageFlip.Vert)
                    textureFlip(imgData, tex.width, tex.height, 0);
                else if (flip == ImageFlip.Hori)
                    textureFlip(imgData, tex.width, tex.height, 1);

                Texture2D texWithDraw = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
                texWithDraw.LoadRawTextureData(imgData);
                texWithDraw.Apply();
                frrTex.tex = texWithDraw;
            }
            else {
                frrTex.tex = tex;
            }

            return frrTex;
        }

        /// <summary>
        /// Do face detection,face alignment,face normalization,face feature extraction for the texture
        /// </summary>
        /// <param name="tex">Process on this texture</param>
        /// <param name="texWithDraw">Draw a landmark or rectangle on this texture</param>
        /// <param name="draw">Whether to draw a landmark or rectangle</param>
        /// <param name="flip">How to mirror the image</param>
        /// <returns>face recognition result</returns>
        public FaceRecognitionResult GetFeatures(Texture2D tex, ref Texture2D texWithDraw, Draw draw = Draw.BOTH, ImageFlip flip = ImageFlip.Vert)
        {
            FaceRecognitionResult frr;

            if (tex == null)
                return BuildErrorResult(ErrCode.TextureError);

            if (texWithDraw.format != TextureFormat.RGBA32 || texWithDraw.height != tex.height || texWithDraw.width != tex.width)
                return BuildErrorResult(ErrCode.TextureError);

            byte[] imgData = Color32ArrayToByteArray(tex.GetPixels32());

            frr = GetFeatures(imgData, tex.width, tex.height, draw, flip);

            if (draw != Draw.NONE) {
                if (flip == ImageFlip.Vert)
                    textureFlip(imgData, tex.width, tex.height, 0);
                else if (flip == ImageFlip.Hori)
                    textureFlip(imgData, tex.width, tex.height, 1);

                texWithDraw.LoadRawTextureData(imgData);
                texWithDraw.Apply();
            }

            return frr;
        }

        /// <summary>
        /// Do face detection,face alignment,face normalization,face feature extraction for the texture
        /// </summary>
        /// <param name="tex">Process on this texture</param>
        /// <param name="flip">How to mirror the image</param>
        /// <returns>face recognition result</returns>
        public FaceRecognitionResult GetFeatures(Texture2D tex, ImageFlip flip = ImageFlip.Vert)
        {
            FaceRecognitionResult frr;

            if (tex == null)
                return BuildErrorResult(ErrCode.TextureError);

            byte[] imgData = Color32ArrayToByteArray(tex.GetPixels32());

            frr = GetFeatures(imgData, tex.width, tex.height, Draw.NONE, flip);
            return frr;
        }

        /// <summary>
        /// Do face detection,face alignment,face normalization,face feature extraction for the picture file
        /// </summary>
        /// <param name="picPath">Process on this picture file</param>
        /// <param name="flip">How to mirror the image</param>
        /// <returns>face recognition result</returns>
        public FaceRecognitionResult GetFeatures(string picPath, ImageFlip flip = ImageFlip.Vert)
        {
            Texture2D tex;
            FaceRecognitionResult frr;

            tex = ReadAsTex(picPath);

            if (tex == null) {
                return BuildErrorResult(ErrCode.TextureError);
            }
            else {
                frr = GetFeatures(tex.GetPixels32(), tex.width, tex.height, flip);
                return frr;
            }
        }

        /// <summary>
        /// Do face detection,face alignment,face normalization,face feature extraction for the color32 array
        /// </summary>
        /// <param name="colors">Process on this Color32 array</param>
        /// <param name="width">Width of the image array</param>
        /// <param name="height">Height of the image array</param>
        /// <param name="flip">How to mirror the image</param>
        /// <returns>face recognition result</returns>
        public FaceRecognitionResult GetFeatures(Color32[] colors, int width, int height, ImageFlip flip = ImageFlip.Vert)
        {
            byte[] imgData;

            imgData = Color32ArrayToByteArray(colors);
            return GetFeatures(imgData, width, height, Draw.NONE, flip);
        }

        /// <summary>
        /// Do face detection,face alignment,face normalization,face feature extraction for the byte array
        /// </summary>
        /// <param name="imgData">Process on this Color32 array</param>
        /// <param name="width">Width of the image array</param>
        /// <param name="height">Height of the image array</param>
        /// <param name="draw">Whether to draw a landmark or rectangle</param>
        /// <param name="flip">How to mirror the image</param>
        /// <returns>face recognition result</returns>
        public FaceRecognitionResult GetFeatures(byte[] imgData, int width, int height, Draw draw = Draw.NONE, ImageFlip flip = ImageFlip.Vert)
        {
            IntPtr pRect = IntPtr.Zero;
            IntPtr pNetFs = IntPtr.Zero;
            int rectNum = 0;
            int netNum = 0;

            if (flip == ImageFlip.Vert)
                textureFlip(imgData, width, height, 0);
            else if (flip == ImageFlip.Hori)
                textureFlip(imgData, width, height, 1);

            Debug.Log("Hola");

            detectAndExtract(_pKFace,
                            imgData,
                            width,
                            height,
                            ref pRect,
                            ref rectNum,
                            ref pNetFs,
                            ref netNum,
                            draw);

            

            KRect[] rectArray;
            rectArray = new KRect[rectNum];

            for (int i = 0; i < rectNum; ++i) {
                var data = new IntPtr(pRect.ToInt64() + Marshal.SizeOf(typeof(KRect)) * i);
                rectArray[i] = (KRect)Marshal.PtrToStructure(data, typeof(KRect));
            }

            MobileFaceNetFeatures[] featureArray;
            featureArray = new MobileFaceNetFeatures[netNum];

            for (int i = 0; i < netNum; ++i) {
                var data = new IntPtr(pNetFs.ToInt64() + Marshal.SizeOf(typeof(MobileFaceNetFeatures)) * i);
                featureArray[i] = (MobileFaceNetFeatures)Marshal.PtrToStructure(data, typeof(MobileFaceNetFeatures));
            }

            freeDetectAlignNormalizeMemory(ref pRect, ref pNetFs);

            FaceRecognitionResult frResult = new FaceRecognitionResult();
            frResult.rectNum = rectNum;
            frResult.rects = rectArray;
            frResult.netNum = netNum;
            frResult.facenetfs = featureArray;
            frResult.errCode = ErrCode.NONE;
            return frResult;
        }

        /// <summary>
        /// Calculate the cosine similarity(https://en.wikipedia.org/wiki/Cosine_similarity) of two faces 
        /// </summary>
        /// <param name="f1">Features struct of a face</param>
        /// <param name="f2">Features struct of another face</param>
        /// <returns>The similarity(from -1 to 1) of two faces, the larger the value is, the more similar it is</returns>
        public double GetSimilarity(MobileFaceNetFeatures f1, MobileFaceNetFeatures f2)
        {
            return computeSimilarity(ref f1, ref f2);
        }

        /// <summary>
        /// Read the picture file into a Unity Texture2D object
        /// </summary>
        /// <param name="picPath">Picture file path</param>
        /// <returns>Texture to the picture</returns>
        public Texture2D ReadAsTex(string picPath)
        {
            byte[] byteArray = File.ReadAllBytes(picPath);
            // create a texture and load byte array to it
            // Texture size does not matter 
            Texture2D sampleTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            // the size of the texture will be replaced by image size
            bool isLoaded = sampleTexture.LoadImage(byteArray);

            if (isLoaded)
                return sampleTexture;
            else
                return null;
        }

        /// <summary>
        /// Building a face recognition error result
        /// </summary>
        /// <param name="code">Error code</param>
        /// <returns>A face recognition error result</returns>
        private FaceRecognitionResult BuildErrorResult(ErrCode code)
        {
            FaceRecognitionResult frr;

            frr.rectNum = -1;
            frr.rects = null;
            frr.netNum = -1;
            frr.facenetfs = null;
            frr.errCode = code;

            return frr;
        }

        /// <summary>
        /// Building a face recognition error result
        /// </summary>
        /// <param name="code">Error code</param>
        /// <returns>A face recognition error result</returns>
        private FaceRecognitionResultWithTex2D BuildErrorResultWithTex2D(ErrCode code)
        {
            FaceRecognitionResultWithTex2D frrTex;
            frrTex.frr = BuildErrorResult(code);
            frrTex.tex = null;
            return frrTex;
        }

        /// <summary>
        /// Convert the Color32 array to bytes array
        /// https://stackoverflow.com/questions/21512259/fast-copy-of-color32-array-to-byte-array
        /// </summary>
        /// <param name="colors">Color32 array</param>
        /// <returns>Bytes array</returns>
        public byte[] Color32ArrayToByteArray(Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            int length = lengthOfColor32 * colors.Length;
            byte[] bytes = new byte[length];

            GCHandle handle = default(GCHandle);
            try {
                handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                System.IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }

        /// <summary>
        /// Convert a face features struct to string
        /// </summary>
        /// <param name="fs">A face features</param>
        /// <returns>XML string</returns>
        public string FeaturesToXMLStr(MobileFaceNetFeatures fs)
        {
            StringWriter stringWriter = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(MobileFaceNetFeatures));
            serializer.Serialize(stringWriter, fs);
            return stringWriter.ToString();
        }

        /// <summary>
        /// Save a face features to file
        /// </summary>
        /// <param name="fs">A face features</param>
        /// <param name="filename">File to save features XML</param>
        public void FeaturesToXMLFile(MobileFaceNetFeatures fs, string filename)
        {
            TextWriter fileWriter = new StreamWriter(filename);
            XmlSerializer serializer = new XmlSerializer(typeof(MobileFaceNetFeatures));
            serializer.Serialize(fileWriter, fs);
            fileWriter.Close();
        }

        /// <summary>
        /// Save a face features to file
        /// </summary>
        /// <param name="fs">A face features</param>
        /// <param name="filename">File to save features XML</param>
        public XmlDocument FeaturesToXMLDocument(MobileFaceNetFeatures fs)
        {
            StringWriter stringWriter = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(MobileFaceNetFeatures));
            serializer.Serialize(stringWriter, fs);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(stringWriter.ToString());

            return xmlDoc;            
        }

        /// <summary>
        /// Convert a XML string to features struct
        /// </summary>
        /// <param name="xml">Features XML string</param>
        /// <returns>A face features struct</returns>
        public MobileFaceNetFeatures XMLStrToFeatures(string xml)
        {
            StringReader stringReader = new StringReader(xml);
            XmlSerializer serializer = new XmlSerializer(typeof(MobileFaceNetFeatures));
            return (MobileFaceNetFeatures)serializer.Deserialize(stringReader);
        }

        /// <summary>
        /// Read a XML file into features struct
        /// </summary>
        /// <param name="filename">face features XML file</param>
        /// <returns>A face features struct</returns>
        public MobileFaceNetFeatures XMLFileToFeatures(string filename)
        {
            MobileFaceNetFeatures fs;
            TextReader fileReader = new StreamReader(filename);
            XmlSerializer serializer = new XmlSerializer(typeof(MobileFaceNetFeatures));
            fs = (MobileFaceNetFeatures)serializer.Deserialize(fileReader);
            fileReader.Close();
            return fs;
        }
    }
}