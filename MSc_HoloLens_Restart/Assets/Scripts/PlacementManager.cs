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
    public TextMeshProUGUI roiCountText;
    
    public GameObject anchor;

    internal SpawnSpheres spawner;
    internal DataManager data_manager;

    Camera mainCam;

    public int latestLODValue = 0;

    public int lowestLOD = 0;

    public bool dynamicLOD = true;
    public float dist = 0;
    public GameObject dynamicLODOnButtonHand;
    public GameObject dynamicLODOnButton;
    public GameObject dynamicLODOffButtonHand;
    public GameObject dynamicLODOffButton;

    internal Coroutine dynamicLODCoroutine;

    public float nearestDistanceSoFar;

    public bool ROI;

    [Header("Query")]
    public GameObject queryObjectArea;
    public GameObject queryObjectNearest;
    public GameObject queryResultPosition;
    public GameObject queryResultTooltip;

    public GameObject solTooltip;
    public GameObject solTooltipTarget;

    public GameObject galacticCenterTooltip;
    public GameObject galacticCenterTooltipTarget;

    public GameObject sourceIDSearchTooltip;
    public GameObject sourceIDSearchTooltipTarget;

    public bool coroutineRunning = false;

    internal Reader reader;

    public GameObject debugSphere;
    public GameObject northPole;
    public GameObject zeroZero;

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
        data_manager = FindObjectOfType<DataManager>();

        ui_manager.SetHUDMapPosition(Vector3.zero, 1f);

        Vector3 newRay = visualCube.transform.right;
        Debug.Log(newRay);

        
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

        if (!ROI && dynamicLOD && data_manager.octree != null)
        {
            if (!coroutineRunning)
            {
                dynamicLODCoroutine = StartCoroutine(data_manager.SplitOctreeLODWithPixelSize());
            }
        }


        if(queryObjectNearest.activeSelf)
        {
            QueryNearest();
        }
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

    public void ToggleDynamicLOD(int boolean)
    {
        if (boolean == 0)
        {
            dynamicLOD = false;
            dynamicLODOffButton.SetActive(false);
            dynamicLODOffButtonHand.SetActive(false);
            dynamicLODOnButton.SetActive(true);
            dynamicLODOnButtonHand.SetActive(true);

            StopCoroutine(dynamicLODCoroutine);
            coroutineRunning = false;
        }
        else if(boolean == 1)
        {
            dynamicLOD = true;
            dynamicLODOffButton.SetActive(true);
            dynamicLODOffButtonHand.SetActive(true);
            dynamicLODOnButton.SetActive(false);
            dynamicLODOnButtonHand.SetActive(false);

            if(ROI)
            {
                queryResultTooltip.SetActive(false);
                queryResultPosition.SetActive(false);

                sourceIDSearchTooltip.SetActive(false);
                sourceIDSearchTooltipTarget.SetActive(false);
            }

            ROI = false;
            ui_manager.SetHUDMapPosition(Vector3.zero, 1f);
            ActivateSolGCTooltip(true);
        }
    }

    public void ToggleDynamicLODAsRPC(int boolean)
    {
        photonView.RPC("RPC_ToogleDynamicLOD", RpcTarget.All, boolean);
    }

    [PunRPC]
    public void RPC_ToogleDynamicLOD(int boolean)
    {
        ToggleDynamicLOD(boolean);
    }

    public void EnableQueryObject()
    {
        queryObjectArea.transform.position = handMenu.transform.position;
        queryObjectArea.SetActive(true);
    }

    public void QueryAreaObject()
    {
        photonView.RPC("RPC_QueryAreaObject", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_QueryAreaObject()
    {
        EnableQueryObject();
    }



    public void EnableQueryObjectNearest()
    {
        //queryObjectNearest.transform.position = handMenu.transform.position;
        queryObjectNearest.SetActive(!queryObjectNearest.activeSelf);
    }

    public void QueryNearestObject()
    {
        photonView.RPC("RPC_QueryNearestObject", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_QueryNearestObject()
    {
        EnableQueryObjectNearest();
    }

    public void ResetCube()
    {
        visualCube.transform.localPosition = new Vector3(0, 0.5f, 0);
        visualCube.transform.localRotation = new Quaternion(0,0,0,0);
        visualCube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    public void QueryTreeWithRadius()
    {
        float radius = queryObjectArea.transform.localScale.x / 2;

        queryObjectArea.SetActive(false);
        List<CelestialBody> results = null;
        if(ROI)
        {
            results = data_manager.ROI_octree.GetListOfNearby(queryObjectArea.transform.localPosition, radius);
        }
        else
        {
            results = data_manager.octree.GetListOfNearby(queryObjectArea.transform.localPosition, radius);
            StopCoroutine(dynamicLODCoroutine);
            coroutineRunning = false;
            ROI = true;
        }

        data_manager.ROI_octree = new PointOctree<CelestialBody>(2, Vector3.zero, 0.000000025f);
        data_manager.ROI_celestialBodyCloud = new CelestialBody[results.Count];

        data_manager.CalculateAverageLists(results, queryObjectArea.transform.localPosition.x, queryObjectArea.transform.localPosition.y, queryObjectArea.transform.localPosition.z, radius);
        ToggleDynamicLOD(0);

        ActivateSolGCTooltip(false);

        ui_manager.SetHUDMapPosition(queryObjectArea.transform.localPosition, radius);

        queryResultTooltip.SetActive(false);
        queryResultPosition.SetActive(false);

        sourceIDSearchTooltip.SetActive(false);
        sourceIDSearchTooltipTarget.SetActive(false);
    }

    public void QueryNearestPoint()
    {
        //queryObjectNearest.SetActive(false);
        nearestDistanceSoFar = float.MaxValue;
        List<CelestialBody> results = null;
        if (ROI)
        {
            results = data_manager.ROI_octree.GetNearest(this, queryObjectNearest.transform.localPosition, float.MaxValue);
        }
        else
        {
            results = data_manager.octree.GetNearest(this, queryObjectNearest.transform.localPosition, float.MaxValue);
        }


        Debug.Log("Nearest result: " + results.Count);

        queryResultPosition.transform.localPosition = results[0].position;
        queryResultPosition.SetActive(true);
        queryResultTooltip.GetComponent<ToolTipConnector>().Target = queryResultPosition;
        queryResultTooltip.SetActive(true);
        queryResultTooltip.GetComponent<ToolTip>().ToolTipText = String.Format("Source ID: {0}\nPosition: {1},{2},{3}\nDistanz: {4} pc\nTemperatur: {5} K\nRadius: {6} Rsun", results[0].source_id, results[0].position.x, results[0].position.y, results[0].position.z, results[0].distance, results[0].temperature, results[0].radius);
    }


    public void QueryArea()
    {
        photonView.RPC("RPC_QueryArea", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_QueryArea()
    {
        QueryTreeWithRadius();
    }

    public void QueryNearest()
    {
        photonView.RPC("RPC_QueryNearest", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_QueryNearest()
    {
        QueryNearestPoint();
    }


    public IEnumerator SearchCelBodyWithSearchID(string searchID)
    {
        int count = 0;
        CelestialBody result = null;
        if (!ROI)
        {
            foreach (CelestialBody celBody in data_manager.celestialBodyCloud)
            {
                count++;
                if(celBody.source_id.ToString() == searchID)
                {
                    result = celBody;
                    break;
                }

                if(count > 2000)
                {
                    count = 0;
                    yield return null;
                }
            }
        }
        else
        {
            foreach (CelestialBody celBody in data_manager.ROI_celestialBodyCloud)
            {
                count++;
                if (celBody.source_id.ToString() == searchID)
                {
                    result = celBody;
                    break;
                }

                if (count > 2000)
                {
                    count = 0;
                    yield return null;
                }
            }
        }

        if(result != null)
        {
            sourceIDSearchTooltipTarget.transform.localPosition = result.position;
            sourceIDSearchTooltipTarget.SetActive(true);
            sourceIDSearchTooltip.SetActive(true);
            sourceIDSearchTooltip.GetComponent<ToolTip>().ToolTipText = String.Format("Source ID: {0}\nPosition: {1},{2},{3}\nDistanz: {4} pc\nTemperatur: {5} K\nRadius: {6} Rsun", result.source_id, result.position.x, result.position.y, result.position.z, result.distance, result.temperature, result.radius);
        }
    }

    public void SearchCelBody()
    {
        photonView.RPC("RPC_SearchCelBody", RpcTarget.All, ui_manager.sourceIDSearchString);
    }

    [PunRPC]
    public void RPC_SearchCelBody(string searchID)
    {
        StartCoroutine(SearchCelBodyWithSearchID(searchID));
    }

    public void PlaceROIWithAngle(float ra, float dec, float distance, float range)
    {
        queryObjectArea.transform.localPosition = zeroZero.transform.localPosition;

        queryObjectArea.transform.RotateAround(visualCube.transform.position, northPole.transform.position - visualCube.transform.position, ra);
        queryObjectArea.transform.RotateAround(visualCube.transform.position, Vector3.Cross(queryObjectArea.transform.localPosition, northPole.transform.position - visualCube.transform.position), dec);

        queryObjectArea.transform.localPosition = queryObjectArea.transform.localPosition.normalized * (distance / data_manager.maxDistance);
        queryObjectArea.transform.localRotation = Quaternion.identity;

        queryObjectArea.transform.localScale = new Vector3(range, range, range);
    }

    public void ToggleSpectralClass(int spectralClass)
    {
        if(spawner == null)
        {
            spawner = FindObjectOfType<SpawnSpheres>();
        }
        switch (spectralClass)
        {
            case 0:
                spawner.particleSystemM.gameObject.SetActive(!spawner.particleSystemM.gameObject.activeSelf);
                if (data_manager.octree == null)
                {
                    return;
                }
                spawner.ApplyToParticleSystem('M', data_manager.averageSpectralM);
                break;
            case 1:
                Debug.Log("toggle 1");
                spawner.particleSystemK.gameObject.SetActive(!spawner.particleSystemK.gameObject.activeSelf);
                if (data_manager.octree == null)
                {
                    return;
                }
                spawner.ApplyToParticleSystem('K', data_manager.averageSpectralK);
                break;
            case 2:
                spawner.particleSystemG.gameObject.SetActive(!spawner.particleSystemG.gameObject.activeSelf);
                if (data_manager.octree == null)
                {
                    return;
                }
                spawner.ApplyToParticleSystem('G', data_manager.averageSpectralG);
                break;
            case 3:
                spawner.particleSystemF.gameObject.SetActive(!spawner.particleSystemF.gameObject.activeSelf);
                if (data_manager.octree == null)
                {
                    return;
                }
                spawner.ApplyToParticleSystem('F', data_manager.averageSpectralF);
                break;
            case 4:
                spawner.particleSystemA.gameObject.SetActive(!spawner.particleSystemA.gameObject.activeSelf);
                if (data_manager.octree == null)
                {
                    return;
                }
                spawner.ApplyToParticleSystem('A', data_manager.averageSpectralA);
                break;
        }
    }

    internal void ActivateSolGCTooltip(bool value)
    {
        solTooltip.SetActive(value);
        solTooltipTarget.SetActive(value);

        galacticCenterTooltip.SetActive(value);
        galacticCenterTooltipTarget.SetActive(value);
    }


    void OnDrawGizmos()
    {
        if (data_manager.nodeList != null && data_manager.nodeList.Count > 0)
        {
            float tintVal = latestLODValue / 7; // Will eventually get values > 1. Color rounds to 1 automatically
            Gizmos.color = new Color(tintVal, 0, 1.0f - tintVal, 0.1f);
            foreach (PointOctreeNode<CelestialBody> node in data_manager.nodeList)
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

            stream.SendNext((Vector3)queryObjectArea.transform.localPosition);
            stream.SendNext((Vector3)queryObjectArea.transform.localScale);

            stream.SendNext((Vector3)queryObjectNearest.transform.localPosition);
            stream.SendNext((Vector3)queryObjectNearest.transform.localScale);
        }
        else
        {
            visualCube.transform.localPosition = (Vector3)stream.ReceiveNext();
            visualCube.transform.localRotation = (Quaternion)stream.ReceiveNext();
            visualCube.transform.localScale = (Vector3)stream.ReceiveNext();

            queryObjectArea.transform.localPosition = (Vector3)stream.ReceiveNext();
            queryObjectArea.transform.localScale = (Vector3)stream.ReceiveNext();

            queryObjectNearest.transform.localPosition = (Vector3)stream.ReceiveNext();
            queryObjectNearest.transform.localScale = (Vector3)stream.ReceiveNext();
        }
    }
}
