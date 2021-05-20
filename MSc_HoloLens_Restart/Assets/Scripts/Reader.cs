using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using DataStructures.ViliWonka.KDTree;
using System.Windows;
using System.Threading.Tasks;
using System.Linq;

public class Reader : MonoBehaviour
{
    //Create a custom struct and apply [Serializable] attribute to it
    

    public TextAsset file;
    internal SpawnSpheres spawner;
    internal HttpFileFetcher http_fetcher;
    internal UIManager ui_manager;

    public string fileName;
    protected BinaryReader binaryReader = null;

    protected TextAsset theSourceFile = null;
    protected StreamReader reader = null;
    protected string text = " "; // assigned to allow first line to be read below

    public Vector3[] pointCloud;

    [SerializeField]
    public CelestialBody[] celestialBodyCloud;
    public List<int> spectralMIndex;
    public List<int> spectralKIndex;
    public List<int> spectralGIndex;
    public List<int> spectralFIndex;
    public List<int> spectralAIndex;

    public List<KDNode> averageSpectralM;
    public List<KDNode> averageSpectralK;
    public List<KDNode> averageSpectralG;
    public List<KDNode> averageSpectralF;
    public List<KDNode> averageSpectralA;
    public List<float> averageSpectralDistanceM;
    public List<float> averageSpectralDistanceK;
    public List<float> averageSpectralDistanceG;
    public List<float> averageSpectralDistanceF;
    public List<float> averageSpectralDistanceA;

    internal KDTree tree;

    public List<Vector3> averagePoints;
    public bool average;

    void Start()
    {
        http_fetcher = FindObjectOfType<HttpFileFetcher>();
        ui_manager = FindObjectOfType<UIManager>();
        spawner = FindObjectOfType<SpawnSpheres>();

        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data");
        //File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data/empty.txt");

        averagePoints = new List<Vector3>();
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
        int arraySize = binaryReader.ReadInt32();
        pointCloud = new Vector3[arraySize];
        celestialBodyCloud = new CelestialBody[arraySize];
        //pointCloud = new Vector3[69593];


        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
        float inputX = 0;
        float inputY = 0;
        float inputZ = 0;
        int inputTemp = 0;

        float galCenX = binaryReader.ReadSingle();
        float galCenY = binaryReader.ReadSingle();
        float galCenZ = binaryReader.ReadSingle();
        int galCenTemp = binaryReader.ReadInt32();
        pointCloud[0] = new Vector3(galCenX, galCenY, galCenZ);
        celestialBodyCloud[0] = new CelestialBody();
        celestialBodyCloud[0].position = new Vector3(galCenX, galCenY, galCenZ);
        celestialBodyCloud[0].temperature = galCenTemp;
        ui_manager.galacticCenter.transform.localPosition = pointCloud[0];
        try
        {
            int i = 1;
            while ((inputX = binaryReader.ReadSingle()) != null && (inputY = binaryReader.ReadSingle()) != null && (inputZ = binaryReader.ReadSingle()) != null && (inputTemp = binaryReader.ReadInt32()) != null)
            {
                celestialBodyCloud[i] = new CelestialBody
                {
                    position = new Vector3(inputX, inputY, inputZ),
                    temperature = inputTemp
                };

                if (inputTemp <= 3700)
                {
                    spectralMIndex.Add(i);
                }
                else if (inputTemp > 3700 && inputTemp <= 5200)
                {
                    spectralKIndex.Add(i);
                }
                else if (inputTemp > 5200 && inputTemp <= 6000)
                {
                    spectralGIndex.Add(i);
                }
                else if (inputTemp > 6000 && inputTemp <= 7500)
                {
                    spectralFIndex.Add(i);
                }
                else if (inputTemp > 7500 && inputTemp <= 10000)
                {
                    spectralAIndex.Add(i);
                }

                pointCloud[i] = new Vector3(inputX, inputY, inputZ);
                i++;
            }
        }
        catch (EndOfStreamException e)
        {
            Debug.Log("Ende der Datei erreicht: " + e.GetType().Name);
        }

        int maxPointsPerLeafNode = 1;
        tree = new KDTree(pointCloud, maxPointsPerLeafNode, this);
        tree.SetRootNodeVecAndTemp();
        Debug.Log(tree.kdNodesCount);
        ui_manager.SetLODSliderMax(tree.depth);
        
        Debug.Log("Root X: " + tree.RootNode.averagePositionOfNodes.x);
        Debug.Log("Root Y: " + tree.RootNode.averagePositionOfNodes.y);
        Debug.Log("Root Z: " + tree.RootNode.averagePositionOfNodes.z);
        Debug.Log("Root Temp: " + tree.RootNode.averageTempOfNodes);

        Debug.Log("RootNeg X: " + tree.RootNode.negativeChild.averagePositionOfNodes.x);
        Debug.Log("RootNeg Y: " + tree.RootNode.negativeChild.averagePositionOfNodes.y);
        Debug.Log("RootNeg Z: " + tree.RootNode.negativeChild.averagePositionOfNodes.z);
        Debug.Log("RootNeg Temp: " + tree.RootNode.negativeChild.averageTempOfNodes);

        Debug.Log("RootPos X: " + tree.RootNode.positiveChild.averagePositionOfNodes.x);
        Debug.Log("RootPos Y: " + tree.RootNode.positiveChild.averagePositionOfNodes.y);
        Debug.Log("RootPos Z: " + tree.RootNode.positiveChild.averagePositionOfNodes.z);
        Debug.Log("RootPos Temp: " + tree.RootNode.positiveChild.averageTempOfNodes);

        binaryReader.Close();
        //Debug.Log(tree.depth);
        //ui_manager.SetLODSlider(tree.depth);
        //Debug.Log(GetNumberOfLeafNodes(tree.RootNode));
        //SpawnSpheres spawner = FindObjectOfType<SpawnSpheres>();
        if (spawner != null)
        {
            Debug.Log("applying to particle system");
            //spawner.ApplyToParticleSystem(pointCloud);
            //spawner.ApplyToParticleSystem('M', spectralMIndex);
            //spawner.ApplyToParticleSystem('K', spectralKIndex);
            //spawner.ApplyToParticleSystem('G', spectralGIndex);
            //spawner.ApplyToParticleSystem('F', spectralFIndex);
            //spawner.ApplyToParticleSystem('A', spectralAIndex);
        }
        //Array.Sort(celestialBodyCloud, new CelestialBodyComparerX());
        yield return null;
    }

    public int GetNumberOfLeafNodes(KDNode node)
    {
        //int count = 0;
        if(node == null)
        {
            return 0;
        }
        if(node.Leaf)
        {
            return 1;
        }
        else
        {
            return GetNumberOfLeafNodes(node.negativeChild) + GetNumberOfLeafNodes(node.positiveChild);
        }
    }

    public void SplitTreeToLOD(int maxDepth)
    {
        averagePoints.Clear();

        Queue<KDNode> nodeStack = new Queue<KDNode>();
        List<KDNode> nodeList = new List<KDNode>();
        //Debug.Log(tree.RootNode.Count);
        KDNode tempNode = tree.RootNode;
        nodeStack.Enqueue(tempNode);
        //Debug.Log(nodeStack);
        while (nodeStack.Count > 0)
        {
            tempNode = nodeStack.Dequeue();


            //Debug.Log(tempNode);
            if (tempNode != null && tempNode.nodeDepth == maxDepth || tempNode.Leaf)
            {
                nodeList.Add(tempNode);
                continue;
            }
            else if (tempNode != null && tempNode.nodeDepth < maxDepth && !tempNode.Leaf)
            {

                if (tempNode.negativeChild.Count != 0)
                {

                    nodeStack.Enqueue(tempNode.negativeChild);
                }
                if (tempNode.positiveChild.Count != 0)
                {

                    nodeStack.Enqueue(tempNode.positiveChild);
                }
            }
        }
        Debug.Log(nodeList.Count);

        averageSpectralM = new List<KDNode>();
        averageSpectralK = new List<KDNode>();
        averageSpectralG = new List<KDNode>();
        averageSpectralF = new List<KDNode>();
        averageSpectralA = new List<KDNode>();

        averageSpectralDistanceM = new List<float>();
        averageSpectralDistanceK = new List<float>();
        averageSpectralDistanceG = new List<float>();
        averageSpectralDistanceF = new List<float>();
        averageSpectralDistanceA = new List<float>();


        foreach (KDNode node in nodeList)
        {
            //Debug.Log(nodeList[0].Count);
            if (node.averageTempOfNodes <= 3700)
            {
                averageSpectralM.Add(node);
                averageSpectralDistanceM.Add(node.distanceFromAverage);
            }
            else if (node.averageTempOfNodes > 3700 && node.averageTempOfNodes <= 5200)
            {
                averageSpectralK.Add(node);
                averageSpectralDistanceK.Add(node.distanceFromAverage);
            }
            else if (node.averageTempOfNodes > 5200 && node.averageTempOfNodes <= 6000)
            {
                averageSpectralG.Add(node);
                averageSpectralDistanceG.Add(node.distanceFromAverage);
            }
            else if (node.averageTempOfNodes > 6000 && node.averageTempOfNodes <= 7500)
            {
                averageSpectralF.Add(node);
                averageSpectralDistanceF.Add(node.distanceFromAverage);
            }
            else if (node.averageTempOfNodes > 7500 && node.averageTempOfNodes <= 10000)
            {
                averageSpectralA.Add(node);
                averageSpectralDistanceA.Add(node.distanceFromAverage);
            }
            //pointsToSpawn.Add(node.averagePositionOfNodes);

            //List<Vector3> pointsInNode = new List<Vector3>();
            //for (int i = node.start; i < node.end; i++)
            //{
            //    pointsInNode.Add(pointCloud[tree.Permutation[i]]);
            //}
            //Vector3 averagePoint = new Vector3(
            //    pointsInNode.Average(x => x.x),
            //    pointsInNode.Average(x => x.y),
            //    pointsInNode.Average(x => x.z));
            //Debug.Log(averagePoint);
            //averagePoints.Add(averagePoint);
        }
        spawner.ApplyToParticleSystem('M', averageSpectralM);
        spawner.ApplyToParticleSystem('K', averageSpectralK);
        spawner.ApplyToParticleSystem('G', averageSpectralG);
        spawner.ApplyToParticleSystem('F', averageSpectralF);
        spawner.ApplyToParticleSystem('A', averageSpectralA);

        ui_manager.SetLegendCount(averageSpectralM.Count, averageSpectralK.Count, averageSpectralG.Count, averageSpectralF.Count, averageSpectralA.Count);
    }

    public void SendToParticleSystem(char spectralClass)
    {
        switch(spectralClass)
        {
            case 'M':
                spawner.ApplyToParticleSystem('M', averageSpectralM);
                break;
            case 'K':
                spawner.ApplyToParticleSystem('K', averageSpectralK);
                break;
            case 'G':
                spawner.ApplyToParticleSystem('G', averageSpectralG);
                break;
            case 'F':
                spawner.ApplyToParticleSystem('F', averageSpectralF);
                break;
            case 'A':
                spawner.ApplyToParticleSystem('A', averageSpectralA);
                break;
        }
    }

    public void ToggleSpectralClass(int spectralClass)
    {
        switch (spectralClass)
        {
            case 0:
                spawner.particleSystemM.gameObject.SetActive(!spawner.particleSystemM.gameObject.activeSelf);
                spawner.ApplyToParticleSystem('M', averageSpectralM);
                break;
            case 1:
                Debug.Log("toggle 1");
                spawner.particleSystemK.gameObject.SetActive(!spawner.particleSystemK.gameObject.activeSelf);
                spawner.ApplyToParticleSystem('K', averageSpectralK);
                break;
            case 2:
                spawner.particleSystemG.gameObject.SetActive(!spawner.particleSystemG.gameObject.activeSelf);
                spawner.ApplyToParticleSystem('G', averageSpectralG);
                break;
            case 3:
                spawner.particleSystemF.gameObject.SetActive(!spawner.particleSystemF.gameObject.activeSelf);
                spawner.ApplyToParticleSystem('F', averageSpectralF);
                break;
            case 4:
                spawner.particleSystemA.gameObject.SetActive(!spawner.particleSystemA.gameObject.activeSelf);
                spawner.ApplyToParticleSystem('A', averageSpectralA);
                break;
        }
    }


}

[Serializable]
public class CelestialBody
{
    public Vector3 position;
    public int temperature;
}

