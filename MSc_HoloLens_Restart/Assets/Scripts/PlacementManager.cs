using DataStructures.ViliWonka.KDTree;
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

            //float dist = Vector3.Distance(visualCube.transform.position, mainCam.transform.position);
            dist = Vector3.Distance(centerCircle.transform.position, mainCam.transform.position);
            

            if(camToCenter.magnitude < (centerCircle.transform.position - visualCube.transform.position).magnitude)
            {
                dist = 0;
            }

            //dist = Mathf.Max(dist, 0.5f);
            float t = dist / 2f;
            lowestLOD = 0;
            int lodValue = Mathf.Max(Mathf.FloorToInt(ui_manager.LODSlider.maxValue - (ui_manager.LODSlider.maxValue * t)), lowestLOD);
            if(lodValue != latestLODValue)
            {
                latestLODValue = lodValue;
                ui_manager.LODSlider.value = lodValue;

                RemapAndApplyToStepSlider(lodValue, 0, 0, ui_manager.stepSlider.SliderStepDivisions, 1);
                //float newStepSliderValue = Remap(lodValue, 0, 0, ui_manager.stepSlider.SliderStepDivisions, 1);
                
                //FindObjectOfType<Reader>().SplitTreeToLOD(lodValue, false);
                FindObjectOfType<Reader>().SplitOctreeLOD(lodValue);
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
        if (spawner == null)
        {
            spawner = FindObjectOfType<SpawnSpheres>();
        }
        Reader reader = FindObjectOfType<Reader>();
        //KDQuery query = new KDQuery();

        //List<int> results = new List<int>();
        float radius = queryObject.transform.localScale.x / 2;

        // spherical query
        //query.Radius(reader.tree, queryObject.transform.localPosition, radius, results);
        queryObject.SetActive(false);

        List<CelestialBody> results = reader.octree.GetListOfNearby(queryObject.transform.localPosition, radius);

        List<Tuple<Vector3, float>> newPointsM = new List<Tuple<Vector3, float>>();
        List<Tuple<Vector3, float>> newPointsK = new List<Tuple<Vector3, float>>();
        List<Tuple<Vector3, float>> newPointsG = new List<Tuple<Vector3, float>>();
        List<Tuple<Vector3, float>> newPointsF = new List<Tuple<Vector3, float>>();
        List<Tuple<Vector3, float>> newPointsA = new List<Tuple<Vector3, float>>();
        foreach (CelestialBody celBody in results)
        {
            Vector3 roiCenterToPoint = celBody.position - queryObject.transform.localPosition;
            float ratio = roiCenterToPoint.magnitude / radius;
            int temperature = celBody.temperature;
            if (temperature <= 3700)
            {
                newPointsM.Add(new Tuple<Vector3, float>(roiCenterToPoint.normalized * ratio, spawner.particleSize));
            }
            else if (temperature > 3700 && temperature <= 5200)
            {
                newPointsK.Add(new Tuple<Vector3, float>(roiCenterToPoint.normalized * ratio, spawner.particleSize));
            }
            else if (temperature > 5200 && temperature <= 6000)
            {
                newPointsG.Add(new Tuple<Vector3, float>(roiCenterToPoint.normalized * ratio, spawner.particleSize));
            }
            else if (temperature > 6000 && temperature <= 7500)
            {
                newPointsF.Add(new Tuple<Vector3, float>(roiCenterToPoint.normalized * ratio, spawner.particleSize));
            }
            else if (temperature > 7500 && temperature <= 10000)
            {
                newPointsA.Add(new Tuple<Vector3, float>(roiCenterToPoint.normalized * ratio, spawner.particleSize));
            }
            //newPoints.Add(roiCenterToPoint.normalized * ratio);
        }
        //roiCountText.text = "Punkte in ROI: " + newPoints.Count;
        //Debug.Log("Query result: " + newPoints.Count);
        dynamicLOD = false;
        spawner.ApplyToParticleSystem('M', newPointsM);
        spawner.ApplyToParticleSystem('K', newPointsK);
        spawner.ApplyToParticleSystem('G', newPointsG);
        spawner.ApplyToParticleSystem('F', newPointsF);
        spawner.ApplyToParticleSystem('A', newPointsA);
    }

    public void QueryNearestPoint()
    {
        if (spawner == null)
        {
            spawner = FindObjectOfType<SpawnSpheres>();
        }
        Reader reader = FindObjectOfType<Reader>();
        nearestDistanceSoFar = float.MaxValue;
        List<CelestialBody> results = reader.octree.GetNearest(this, queryObject.transform.localPosition, float.MaxValue);

        Debug.Log("Nearest result: " + results.Count);

        queryResultPosition.transform.localPosition = results[0].position;
        queryResultPosition.SetActive(true);
        queryResultTooltip.GetComponent<ToolTipConnector>().Target = queryResultPosition;
        queryResultTooltip.GetComponent<ToolTip>().ToolTipText = results[0].position + "\n" + results[0].temperature;
        queryResultTooltip.SetActive(true);
    }

    void OnDrawGizmos()
    {
        if (FindObjectOfType<Reader>().nodeList != null)
        {
            float tintVal = latestLODValue / 7; // Will eventually get values > 1. Color rounds to 1 automatically
            Gizmos.color = new Color(tintVal, 0, 1.0f - tintVal, 0.1f);
            //Debug.Log("Gizmo nodeList count:" + FindObjectOfType<Reader>().nodeList.Count);
            foreach (PointOctreeNode<CelestialBody> node in FindObjectOfType<Reader>().nodeList)
            {
                Bounds thisBounds = new Bounds(node.Center, new Vector3(node.SideLength, node.SideLength, node.SideLength));
                Gizmos.DrawWireCube((thisBounds.center/2) + visualCube.transform.position, thisBounds.size / 2f);
            }
        }
        //Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
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
