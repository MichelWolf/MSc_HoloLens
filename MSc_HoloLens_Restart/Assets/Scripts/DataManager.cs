using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField]
    public CelestialBody[] celestialBodyCloud;

    [SerializeField]
    public CelestialBody[] ROI_celestialBodyCloud;

    public List<Tuple<Vector3, float>> averageSpectralM;
    public List<Tuple<Vector3, float>> averageSpectralK;
    public List<Tuple<Vector3, float>> averageSpectralG;
    public List<Tuple<Vector3, float>> averageSpectralF;
    public List<Tuple<Vector3, float>> averageSpectralA;

    internal PointOctree<CelestialBody> octree;
    internal PointOctree<CelestialBody> ROI_octree;
    internal List<PointOctreeNode<CelestialBody>> nodeList;

    internal UIManager ui_manager;
    internal PlacementManager placement_manager;
    internal SpawnSpheres spawner;

    public Vector3 screenCenterPos;
    public float maxDistance;

    public void Start()
    {
        ui_manager = FindObjectOfType<UIManager>();
        placement_manager = FindObjectOfType<PlacementManager>();

        averageSpectralM = new List<Tuple<Vector3, float>>();
        averageSpectralK = new List<Tuple<Vector3, float>>();
        averageSpectralG = new List<Tuple<Vector3, float>>();
        averageSpectralF = new List<Tuple<Vector3, float>>();
        averageSpectralA = new List<Tuple<Vector3, float>>();

        //octree = new PointOctree<CelestialBody>(2, Vector3.zero, 0.000000025f);
        //ROI_octree = new PointOctree<CelestialBody>(2, Vector3.zero, 0.000000025f);
    }

    public void SplitOctreeLOD(int maxDepth)
    {
        Queue<PointOctreeNode<CelestialBody>> nodeQueue = new Queue<PointOctreeNode<CelestialBody>>();
        nodeList = new List<PointOctreeNode<CelestialBody>>();

        PointOctreeNode<CelestialBody> tempNode = octree.rootNode;

        nodeQueue.Enqueue(tempNode);

        while (nodeQueue.Count > 0)
        {
            tempNode = nodeQueue.Dequeue();


            //Debug.Log(tempNode);
            if (tempNode != null && tempNode.nodeDepth == maxDepth || !tempNode.HasChildren)
            {
                nodeList.Add(tempNode);
                continue;
            }
            else if (tempNode != null && tempNode.nodeDepth < maxDepth && tempNode.HasChildren)
            {
                foreach (PointOctreeNode<CelestialBody> childNode in tempNode.children)
                {
                    nodeQueue.Enqueue(childNode);
                }
            }
        }

        CalculateAverageLists(nodeList);

        ui_manager.SetLegendCount(averageSpectralM.Count, averageSpectralK.Count, averageSpectralG.Count, averageSpectralF.Count, averageSpectralA.Count);
    }


    public IEnumerator SplitOctreeLODWithPixelSize()
    {
        placement_manager.coroutineRunning = true;
        for (; ; )
        {
            int count = 0;
            
            Queue<PointOctreeNode<CelestialBody>> nodeQueue = new Queue<PointOctreeNode<CelestialBody>>();
            nodeList = new List<PointOctreeNode<CelestialBody>>();

            PointOctreeNode<CelestialBody> tempNode = octree.rootNode;

            nodeQueue.Enqueue(tempNode);
            while (nodeQueue.Count > 0)
            {
                tempNode = nodeQueue.Dequeue();

                count++;

                if (count > 2000)
                {
                    count = 0;
                    yield return null;
                }
                //Debug.Log(tempNode);
                if (tempNode != null && !tempNode.HasChildren)
                {

                    nodeList.Add(tempNode);
                    continue;
                }
                else if (tempNode != null && tempNode.HasChildren)
                {
                    //nodeList.Add(tempNode);
                    Vector3 centerPos = placement_manager.visualCube.transform.position + tempNode.averagePositionOfNodes;
                    Vector3 edgePos = centerPos + (Vector3.Cross(Camera.main.transform.up, centerPos - Camera.main.transform.position).normalized * (tempNode.distanceFromAverage / 2) * placement_manager.visualCube.transform.localScale.x);
                    //screenCenterPos = Camera.main.WorldToScreenPoint(centerPos);
                    float distanceInPixel = Vector3.Magnitude(Camera.main.WorldToScreenPoint(centerPos) - Camera.main.WorldToScreenPoint(edgePos));

                    if (distanceInPixel > 2)
                    {
                        foreach (PointOctreeNode<CelestialBody> childNode in tempNode.children)
                        {
                            nodeQueue.Enqueue(childNode);
                        }
                    }
                    else
                    {
                        nodeList.Add(tempNode);
                        continue;
                    }
                }
            }
            CalculateAverageLists(nodeList);

            ui_manager.SetLegendCount(averageSpectralM.Count, averageSpectralK.Count, averageSpectralG.Count, averageSpectralF.Count, averageSpectralA.Count);
            yield return new WaitForSeconds(1f);
            Debug.Log(count);

            //count = 0;
            ////    yield return null;
        }
    }


    public void CalculateAverageLists<T>(List<T> list, float x = 0, float y = 0, float z = 0, float radius = -1)
    {
        if (spawner == null)
        {
            spawner = FindObjectOfType<SpawnSpheres>();
        }
        averageSpectralM.Clear();
        averageSpectralK.Clear();
        averageSpectralG.Clear();
        averageSpectralF.Clear();
        averageSpectralA.Clear();

        for (int i = 0; i < list.Count; i++)
        {
            List<Vector3> position = new List<Vector3>();
            List<int> temperature = new List<int>();
            List<float> size = new List<float>();
            if (typeof(T) == typeof(CelestialBody))
            {
                if (radius == -1)
                {
                    position.Add((list[i] as CelestialBody).position);
                }
                else
                {
                    Vector3 roiCenterToPoint = (list[i] as CelestialBody).position - new Vector3(x, y, z);
                    float ratio = roiCenterToPoint.magnitude / radius;
                    Vector3 newPosition = roiCenterToPoint.normalized * ratio;

                    ROI_celestialBodyCloud[i] = new CelestialBody
                    {
                        position = newPosition,
                        temperature = (list[i] as CelestialBody).temperature,
                        distance = (list[i] as CelestialBody).distance,
                        radius = (list[i] as CelestialBody).radius,
                        source_id = (list[i] as CelestialBody).source_id
                    };
                    ROI_octree.Add(ROI_celestialBodyCloud[i],ROI_celestialBodyCloud[i].position);
                    position.Add(newPosition);
                }
                temperature.Add((list[i] as CelestialBody).temperature);
                size.Add(spawner.particleSize);
            }
            if (typeof(T) == typeof(PointOctreeNode<CelestialBody>))
            {
                if (!(list[i] as PointOctreeNode<CelestialBody>).HasChildren)
                {
                    foreach (PointOctreeNode<CelestialBody>.OctreeObject cel in (list[i] as PointOctreeNode<CelestialBody>).objects)
                    {
                        position.Add(cel.Obj.position);
                        temperature.Add(cel.Obj.temperature);
                        size.Add(spawner.particleSize);
                    }
                }
                else
                {
                    position.Add((list[i] as PointOctreeNode<CelestialBody>).averagePositionOfNodes);
                    temperature.Add((list[i] as PointOctreeNode<CelestialBody>).averageTempOfNodes);
                    size.Add((list[i] as PointOctreeNode<CelestialBody>).distanceFromAverage);
                }
            }
            for (int j = 0; j < temperature.Count; j++)
            {
                if (temperature[j] <= 3700)
                {
                    averageSpectralM.Add(new Tuple<Vector3, float>(position[j], size[j]));
                }
                else if (temperature[j] > 3700 && temperature[j] <= 5200)
                {
                    averageSpectralK.Add(new Tuple<Vector3, float>(position[j], size[j]));
                }
                else if (temperature[j] > 5200 && temperature[j] <= 6000)
                {
                    averageSpectralG.Add(new Tuple<Vector3, float>(position[j], size[j]));
                }
                else if (temperature[j] > 6000 && temperature[j] <= 7500)
                {
                    averageSpectralF.Add(new Tuple<Vector3, float>(position[j], size[j]));
                }
                else if (temperature[j] > 7500 && temperature[j] <= 10000)
                {
                    averageSpectralA.Add(new Tuple<Vector3, float>(position[j], size[j]));
                }
            }
        }

        spawner.ApplyToParticleSystem('M', averageSpectralM);
        spawner.ApplyToParticleSystem('K', averageSpectralK);
        spawner.ApplyToParticleSystem('G', averageSpectralG);
        spawner.ApplyToParticleSystem('F', averageSpectralF);
        spawner.ApplyToParticleSystem('A', averageSpectralA);
    }


}
