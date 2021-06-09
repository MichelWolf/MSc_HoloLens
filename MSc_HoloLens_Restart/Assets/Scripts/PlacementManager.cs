using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

public class PlacementManager : MonoBehaviour, IPunObservable
{

    internal UIManager ui_manager;
    public GameObject parentCube;
    public GameObject visualCube;
    internal Vector3 visualScale;

    public GameObject handMenu;
    public GameObject queryObject;
    public TextMeshProUGUI roiCountText;

    public GameObject centerCircle;

    public GameObject anchor;

    internal SpawnSpheres spawner;

    Camera mainCam;

    public int latestLODValue = 0;

    public int lowestLOD = 0;

    public bool dynamicLOD = true;
    public float dist = 0;
    public GameObject dynamicLODOnButton;
    public GameObject dynamicLODOffButton;

    public GameObject queryResultPosition;
    public GameObject queryResultTooltip;

    public float nearestDistanceSoFar;

    public Reader reader;

    PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        visualScale = new Vector3(0.5f, 0.5f, 0.5f);
        ui_manager = FindObjectOfType<UIManager>();
        parentCube.SetActive(true);
        visualCube.SetActive(false);

        photonView = GetComponent<PhotonView>();
        spawner = FindObjectOfType<SpawnSpheres>();
        mainCam = Camera.main;
        reader = FindObjectOfType<Reader>();
    }

    // Update is called once per frame
    void Update()
    {
        if(visualCube.transform.localScale != visualScale)
        {
            visualScale = visualCube.transform.localScale;
            if(spawner == null)
            {
                spawner = FindObjectOfType<SpawnSpheres>();
            }
            spawner.particleSystemM.transform.localScale = visualScale;
            spawner.particleSystemK.transform.localScale = visualScale;
            spawner.particleSystemG.transform.localScale = visualScale;
            spawner.particleSystemF.transform.localScale = visualScale;
            spawner.particleSystemA.transform.localScale = visualScale;
        }

        if (dynamicLOD && FindObjectOfType<Reader>().octree != null)
        {
            Vector3 camToCenter = (mainCam.transform.position - visualCube.transform.position);
            centerCircle.transform.localPosition = camToCenter.normalized * 0.5f;

            dist = Vector3.Distance(centerCircle.transform.position, mainCam.transform.position);
            

            if(camToCenter.magnitude < (centerCircle.transform.position - visualCube.transform.position).magnitude)
            {
                dist = 0;
            }

            float t = dist / 2f;
            lowestLOD = 0;
            int lodValue = Mathf.Max(Mathf.FloorToInt(ui_manager.LODSlider.maxValue - (ui_manager.LODSlider.maxValue * t)), lowestLOD);
            if(lodValue != latestLODValue)
            {
                latestLODValue = lodValue;
                ui_manager.LODSlider.value = lodValue;

                RemapAndApplyToStepSlider(lodValue, 0, 0, ui_manager.stepSlider.SliderStepDivisions, 1);
                SplitOctreeLOD(lodValue);
            }
        }
    }

    private void RemapAndApplyToStepSlider(int lodValue, int fromMin, int toMin, int fromMax, int toMax)
    {
        float fromAbs = lodValue - fromMin;
        float fromMaxAbs = fromMax - fromMin;

        float normal = fromAbs / fromMaxAbs;

        float toMaxAbs = toMax - toMin;
        float toAbs = toMaxAbs * normal;

        float to = toAbs + toMin;

        ui_manager.stepSlider.SliderValue = to;
    }

    public void EnableAdjustment()
    {
        ui_manager.ToogleAdjustState(true);
        //parentCube.SetActive(true);
        visualCube.SetActive(false);

        MonoBehaviour[] compsParent = parentCube.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in compsParent)
        {
            c.enabled = true;
        }
        parentCube.GetComponent<BoxCollider>().enabled = true;

        MonoBehaviour[] compsVisual = visualCube.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in compsVisual)
        {
            c.enabled = false;
        }
        anchor.SetActive(true);
    }

    public void DisableAdjustment()
    {
        ui_manager.ToogleAdjustState(false);
        //parentCube.SetActive(false);
        visualCube.SetActive(true);

        MonoBehaviour[] compsParent = parentCube.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in compsParent)
        {
            c.enabled = false;
        }
        parentCube.GetComponent<BoxCollider>().enabled = false;

        MonoBehaviour[] compsVisual = visualCube.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in compsVisual)
        {
            c.enabled = true;
        }
        anchor.SetActive(false);
    }

    public void ToggleDynamicLOD()
    {
        dynamicLOD = !dynamicLOD;
        dynamicLODOffButton.SetActive(dynamicLOD);
        dynamicLODOnButton.SetActive(!dynamicLOD);
        ui_manager.stepSlider.enabled = !dynamicLOD;
    }

    public void EnableQueryObject()
    {
        queryObject.transform.position = handMenu.transform.position;
        queryObject.SetActive(true);
    }

    public void ResetCube()
    {
        visualCube.transform.localPosition = new Vector3(0, 0.5f, 0);
        visualCube.transform.localRotation = new Quaternion(0,0,0,0);
        visualCube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    public void QueryTreeWithRadius()
    {
        float radius = queryObject.transform.localScale.x / 2;

        queryObject.SetActive(false);

        List<CelestialBody> results = reader.octree.GetListOfNearby(queryObject.transform.localPosition, radius);

        CalculateAverageLists(results, queryObject.transform.localPosition.x, queryObject.transform.localPosition.y, queryObject.transform.localPosition.z, radius);
        dynamicLOD = false;
    }

    public void QueryNearestPoint()
    {
       
        nearestDistanceSoFar = float.MaxValue;
        List<CelestialBody> results = reader.octree.GetNearest(this, queryObject.transform.localPosition, float.MaxValue);

        Debug.Log("Nearest result: " + results.Count);

        queryResultPosition.transform.localPosition = results[0].position;
        queryResultPosition.SetActive(true);
        queryResultTooltip.GetComponent<ToolTipConnector>().Target = queryResultPosition;
        queryResultTooltip.GetComponent<ToolTip>().ToolTipText = results[0].position + "\n" + results[0].temperature;
        queryResultTooltip.SetActive(true);
    }

    public void SplitOctreeLOD(int maxDepth)
    {
        Queue<PointOctreeNode<CelestialBody>> nodeQueue = new Queue<PointOctreeNode<CelestialBody>>();
        reader.nodeList = new List<PointOctreeNode<CelestialBody>>();

        PointOctreeNode<CelestialBody> tempNode = reader.octree.rootNode;

        nodeQueue.Enqueue(tempNode);

        while (nodeQueue.Count > 0)
        {
            tempNode = nodeQueue.Dequeue();


            //Debug.Log(tempNode);
            if (tempNode != null && tempNode.nodeDepth == maxDepth || !tempNode.HasChildren)
            {
                reader.nodeList.Add(tempNode);
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

        CalculateAverageLists(reader.nodeList);

        ui_manager.SetLegendCount(reader.averageSpectralM.Count, reader.averageSpectralK.Count, reader.averageSpectralG.Count, reader.averageSpectralF.Count, reader.averageSpectralA.Count);
    }

    public void CalculateAverageLists<T>(List<T> list, float x = 0, float y = 0, float z = 0, float radius = -1)
    {
        if (spawner == null)
        {
            spawner = FindObjectOfType<SpawnSpheres>();
        }
        reader.averageSpectralM.Clear();
        reader.averageSpectralK.Clear();
        reader.averageSpectralG.Clear();
        reader.averageSpectralF.Clear();
        reader.averageSpectralA.Clear();

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
                    reader.averageSpectralM.Add(new Tuple<Vector3, float>(position[j], size[j]));
                }
                else if (temperature[j] > 3700 && temperature[j] <= 5200)
                {
                    reader.averageSpectralK.Add(new Tuple<Vector3, float>(position[j], size[j]));
                }
                else if (temperature[j] > 5200 && temperature[j] <= 6000)
                {
                    reader.averageSpectralG.Add(new Tuple<Vector3, float>(position[j], size[j]));
                }
                else if (temperature[j] > 6000 && temperature[j] <= 7500)
                {
                    reader.averageSpectralF.Add(new Tuple<Vector3, float>(position[j], size[j]));
                }
                else if (temperature[j] > 7500 && temperature[j] <= 10000)
                {
                    reader.averageSpectralA.Add(new Tuple<Vector3, float>(position[j], size[j]));
                }
            }
        }

        spawner.ApplyToParticleSystem('M', reader.averageSpectralM);
        spawner.ApplyToParticleSystem('K', reader.averageSpectralK);
        spawner.ApplyToParticleSystem('G', reader.averageSpectralG);
        spawner.ApplyToParticleSystem('F', reader.averageSpectralF);
        spawner.ApplyToParticleSystem('A', reader.averageSpectralA);
    }


    public void ToggleSpectralClass(int spectralClass)
    {
        switch (spectralClass)
        {
            case 0:
                spawner.particleSystemM.gameObject.SetActive(!spawner.particleSystemM.gameObject.activeSelf);
                spawner.ApplyToParticleSystem('M', reader.averageSpectralM);
                break;
            case 1:
                Debug.Log("toggle 1");
                spawner.particleSystemK.gameObject.SetActive(!spawner.particleSystemK.gameObject.activeSelf);
                spawner.ApplyToParticleSystem('K', reader.averageSpectralK);
                break;
            case 2:
                spawner.particleSystemG.gameObject.SetActive(!spawner.particleSystemG.gameObject.activeSelf);
                spawner.ApplyToParticleSystem('G', reader.averageSpectralG);
                break;
            case 3:
                spawner.particleSystemF.gameObject.SetActive(!spawner.particleSystemF.gameObject.activeSelf);
                spawner.ApplyToParticleSystem('F', reader.averageSpectralF);
                break;
            case 4:
                spawner.particleSystemA.gameObject.SetActive(!spawner.particleSystemA.gameObject.activeSelf);
                spawner.ApplyToParticleSystem('A', reader.averageSpectralA);
                break;
        }
    }


    void OnDrawGizmos()
    {
        if (FindObjectOfType<Reader>().nodeList != null)
        {
            float tintVal = latestLODValue / 7; // Will eventually get values > 1. Color rounds to 1 automatically
            Gizmos.color = new Color(tintVal, 0, 1.0f - tintVal, 0.1f);
            foreach (PointOctreeNode<CelestialBody> node in FindObjectOfType<Reader>().nodeList)
            {
                Bounds thisBounds = new Bounds(node.Center, new Vector3(node.SideLength, node.SideLength, node.SideLength));
                Gizmos.DrawWireCube((thisBounds.center/2) + visualCube.transform.position, thisBounds.size / 2f);
            }
        }
    }


    public void TakeOwnershipOfView()
    {
        if (!photonView.IsMine)
        {
            photonView.RequestOwnership();
        }
    }

    public void GiveOwnershipToMasterOfView()
    {
        photonView.TransferOwnership(PhotonNetwork.MasterClient);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting == true)
        {
            stream.SendNext((Vector3)visualCube.transform.localPosition);
            stream.SendNext((Quaternion)visualCube.transform.localRotation);
            stream.SendNext((Vector3)visualCube.transform.localScale);
           
        }
        else
        {
            visualCube.transform.localPosition = (Vector3)stream.ReceiveNext();
            visualCube.transform.localRotation = (Quaternion)stream.ReceiveNext();
            visualCube.transform.localScale = (Vector3)stream.ReceiveNext();
        }
    }
}
