using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using System.Linq;

public class Reader : MonoBehaviour
{
    //Create a custom struct and apply [Serializable] attribute to it
    
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

    public List<Tuple<Vector3, float>> averageSpectralM;
    public List<Tuple<Vector3, float>> averageSpectralK;
    public List<Tuple<Vector3, float>> averageSpectralG;
    public List<Tuple<Vector3, float>> averageSpectralF;
    public List<Tuple<Vector3, float>> averageSpectralA;

    internal PointOctree<CelestialBody> octree;
    public List<PointOctreeNode<CelestialBody>> nodeList;
    //internal KDTree tree;
    
    void Start()
    {
        http_fetcher = FindObjectOfType<HttpFileFetcher>();
        ui_manager = FindObjectOfType<UIManager>();
        spawner = FindObjectOfType<SpawnSpheres>();

        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data");
        //File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data/empty.txt");
        averageSpectralM = new List<Tuple<Vector3, float>>();
        averageSpectralK = new List<Tuple<Vector3, float>>();
        averageSpectralG = new List<Tuple<Vector3, float>>();
        averageSpectralF = new List<Tuple<Vector3, float>>();
        averageSpectralA = new List<Tuple<Vector3, float>>();
    }

    public void ReadFile()
    {
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
        octree = new PointOctree<CelestialBody>(2, Vector3.zero, 0.000000025f);

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
                
                octree.Add(celestialBodyCloud[i], celestialBodyCloud[i].position);
                pointCloud[i] = new Vector3(inputX, inputY, inputZ);
                i++;
            }
        }
        catch (EndOfStreamException e)
        {
            Debug.Log("Ende der Datei erreicht: " + e.GetType().Name);
        }

        binaryReader.Close();
                     
        ui_manager.loadDataButton.SetActive(false);

        yield return null;
        Debug.Log(celestialBodyCloud.Length);
        octree.SetRootNodeVecAndTemp();

        ui_manager.SetLODSliderMax(octree.depth);
        

    }


    //public IEnumerator FindLowestLODLevel()
    //{
    //    for(int i = 0; i < tree.depth; i++)
    //    {
    //        SplitTreeToLOD(i, true);

    //        int sumOfPoints = averageSpectralM.Count + averageSpectralK.Count + averageSpectralG.Count + averageSpectralF.Count + averageSpectralA.Count;

    //        if (sumOfPoints > (float)tree.Count * 0.05f)
    //        {
    //            FindObjectOfType<PlacementManager>().lowestLOD = (i - 1);
    //            break;
    //        }

    //        yield return new WaitForEndOfFrame();
    //    }
    //}

    
}

[Serializable]
public class CelestialBody
{
    public Vector3 position;
    public int temperature;
}

