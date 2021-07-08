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

    internal DataManager data_manager;
    
    void Start()
    {
        http_fetcher = FindObjectOfType<HttpFileFetcher>();
        ui_manager = FindObjectOfType<UIManager>();
        data_manager = FindObjectOfType<DataManager>();
        spawner = FindObjectOfType<SpawnSpheres>();

        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data");
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
        if(data_manager == null)
        {
            data_manager = FindObjectOfType<DataManager>();
        }
        int arraySize = binaryReader.ReadInt32();
        
        data_manager.celestialBodyCloud = new CelestialBody[arraySize];
        data_manager.octree = new PointOctree<CelestialBody>(2, Vector3.zero, 0.000000025f);

        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
        float inputX = 0;
        float inputY = 0;
        float inputZ = 0;
        int inputTemp = 0;
        float inputDistance = 0;
        float inputRadius = 0;
        long inputSourceID = 0;

        float galCenX = binaryReader.ReadSingle();
        float galCenY = binaryReader.ReadSingle();
        float galCenZ = binaryReader.ReadSingle();
        int galCenTemp = binaryReader.ReadInt32();
        float galCenInputDistance = binaryReader.ReadSingle();
        float galCeninputRadius = binaryReader.ReadSingle();
        long glaCenInputSourceID = binaryReader.ReadInt64();

        data_manager.celestialBodyCloud[0] = new CelestialBody
        {
            position = new Vector3(galCenX, galCenY, galCenZ),
            temperature = galCenTemp,
            distance = galCenInputDistance,
            radius = galCeninputRadius,
            source_id = glaCenInputSourceID
        };

        data_manager.maxDistance = galCenInputDistance;

        ui_manager.galacticCenter.transform.localPosition = data_manager.celestialBodyCloud[0].position;
        FindObjectOfType<PlacementManager>().ActivateSolGCTooltip(true);

        try
        {
            int i = 1;
            while ((inputX = binaryReader.ReadSingle()) != null && (inputY = binaryReader.ReadSingle()) != null && (inputZ = binaryReader.ReadSingle()) != null && (inputTemp = binaryReader.ReadInt32()) != null && (inputDistance = binaryReader.ReadSingle()) != null && (inputRadius = binaryReader.ReadSingle()) != null && (inputSourceID = binaryReader.ReadInt64()) != null)
            {
                data_manager.celestialBodyCloud[i] = new CelestialBody
                {
                    position = new Vector3(inputX, inputY, inputZ),
                    temperature = inputTemp,
                    distance = inputDistance,
                    radius = inputRadius,
                    source_id = inputSourceID
                };

                data_manager.octree.Add(data_manager.celestialBodyCloud[i], data_manager.celestialBodyCloud[i].position);

                if(data_manager.maxDistance < inputDistance)
                {
                    data_manager.maxDistance = inputDistance;
                }
                
                i++;
            }
        }
        catch (EndOfStreamException e)
        {
            Debug.Log("Ende der Datei erreicht: " + e.GetType().Name);
        }

        binaryReader.Close();

        ui_manager.DisableLoadDataButton(); ;

        yield return null;

        data_manager.octree.SetRootNodeVecAndTemp();

        //ui_manager.SetLODSliderMax(data_manager.octree.depth);

        //FindObjectOfType<PlacementManager>().PlaceDebugSphere(266.4051f, -28.93175f, 8122f);
        //FindObjectOfType<PlacementManager>().PlaceDebugSphere(0, 90, data_manager.maxDistance / 2f);
    }    
}

[Serializable]
public class CelestialBody
{
    public Vector3 position;
    public int temperature;
    public float distance;
    public float radius;
    public long source_id;
}

