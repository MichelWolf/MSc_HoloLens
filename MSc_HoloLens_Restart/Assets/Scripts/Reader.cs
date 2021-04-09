using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using DataStructures.ViliWonka.KDTree;
using System.Windows;
using System.Threading.Tasks;

public class Reader : MonoBehaviour
{
    public TextAsset file;
    internal SpawnSpheres spawner;
    internal HttpFileFetcher http_fetcher;

    public string fileName;
    protected BinaryReader binaryReader = null;

    protected TextAsset theSourceFile = null;
    protected StreamReader reader = null;
    protected string text = " "; // assigned to allow first line to be read below

    public Vector3[] pointCloud;

    internal KDTree tree;

    void Start()
    {
        http_fetcher = FindObjectOfType<HttpFileFetcher>();

        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data");
        //File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data/empty.txt");
    }

    void Update()
    {
        
    }

    public void ReadFile()
    {
        //if (http_fetcher.source == HttpFileFetcher.selectedSource.Server)
        //{

            
            
        //}
        ////binaryReader = new BinaryReader(File.Open(Application.dataPath + "/StreamingAssets/" + fileName, FileMode.Open));
        //binaryReader = new BinaryReader(File.Open(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data/" + fileName, FileMode.Open));
        //StartCoroutine(ReadFileCoroutine());
        ReadFileAsync();
    }

    public async Task ReadFileAsync()
    {
        if (http_fetcher.source == HttpFileFetcher.selectedSource.Server)
        { 
            await http_fetcher.FetchFile(fileName);
        }
        binaryReader = new BinaryReader(File.Open(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data/" + fileName, FileMode.Open));
        StartCoroutine(ReadFileCoroutine());
    }

    public IEnumerator ReadFileCoroutine()
    {
        pointCloud = new Vector3[binaryReader.ReadInt32()];
        //pointCloud = new Vector3[69593];


        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
        float inputX = 0;
        float inputY = 0;
        float inputZ = 0;
        try
        {
            int i = 0;
            while ((inputX = binaryReader.ReadSingle()) != null && (inputY = binaryReader.ReadSingle()) != null && (inputZ = binaryReader.ReadSingle()) != null)
            {
                pointCloud[i] = new Vector3(inputX, inputY, inputZ);
                i++;
            }
        }
        catch (EndOfStreamException e)
        {
            Debug.Log("Ende der Datei erreicht: " + e.GetType().Name);
        }

        //double dX = binaryReader.ReadDouble();
        //double dY = binaryReader.ReadDouble();
        //double dZ = binaryReader.ReadDouble();
        //Debug.Log(dX);
        //Debug.Log(dY);
        //Debug.Log(dZ);
        //string textX = reader.ReadLine();
        //string textY = reader.ReadLine();
        //string textZ = reader.ReadLine();
        //float x = 0;
        //float y = 0;
        //float z = 0;
        //int i = 0;
        ////if (textX != null && textY != null && textZ != null)
        ////{
        ////    x = float.Parse(textX, NumberStyles.Any, ci);
        ////    y = float.Parse(textY, NumberStyles.Any, ci);
        ////    z = float.Parse(textZ, NumberStyles.Any, ci);

        ////    pointCloud[i] = new Vector3(x, y, z);
        ////    i++;
        ////}

        ////while (textX != null && textY != null && textZ != null)
        ////{

        ////    textX = reader.ReadLine();
        ////    textY = reader.ReadLine();
        ////    textZ = reader.ReadLine();

        ////    if (textX != null && textY != null && textZ != null)
        ////    {

        ////        x = float.Parse(textX, NumberStyles.Any, ci);
        ////        y = float.Parse(textY, NumberStyles.Any, ci);
        ////        z = float.Parse(textZ, NumberStyles.Any, ci);



        ////        pointCloud[i] = new Vector3(x, y, z);
        ////        i++;
        ////    }
        ////}
        ////spawner.ApplyToParticleSystem(pointCloud);

        int maxPointsPerLeafNode = 32;
        tree = new KDTree(pointCloud, maxPointsPerLeafNode);
        //Debug.Log(tree.kdNodesCount);

        //Debug.Log(tree.RootNode.bounds.size);
        //Debug.Log(tree.RootNode.negativeChild.bounds.min);
        //Debug.Log(tree.RootNode.negativeChild.bounds.max);
        //Debug.Log(tree.RootNode.negativeChild.Count);
        //Debug.Log("");
        //Debug.Log(tree.RootNode.positiveChild.bounds.min);
        //Debug.Log(tree.RootNode.positiveChild.bounds.max);
        //Debug.Log(tree.RootNode.positiveChild.Count);
        //SplitTreeToLOD(50);
        binaryReader.Close();
        SpawnSpheres spawner = FindObjectOfType<SpawnSpheres>();
        if (spawner != null)
        {
            spawner.ApplyToParticleSystem(pointCloud);
        }
        yield return null;
    }

}
