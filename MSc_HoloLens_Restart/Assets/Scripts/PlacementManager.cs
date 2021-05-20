using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour, IPunObservable
{

    internal UIManager ui_manager;
    public GameObject parentCube;
    public GameObject visualCube;
    internal Vector3 visualScale;

    public GameObject centerCircle;

    public GameObject anchor;

    internal SpawnSpheres spawner;

    Camera mainCam;

    public int latestLODValue = 0;

    public bool dynamicLOD = true;

    public GameObject dynamicLODOnButton;
    public GameObject dynamicLODOffButton;

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

        if (dynamicLOD && FindObjectOfType<Reader>().tree != null)
        {
            centerCircle.transform.localPosition = (mainCam.transform.position - visualCube.transform.position).normalized * 0.1f;
            //float dist = Vector3.Distance(visualCube.transform.position, mainCam.transform.position);
            float dist = Vector3.Distance(centerCircle.transform.position, mainCam.transform.position);
            //dist = Mathf.Max(dist, 0.5f);
            float t = dist / 2f;

            int lodValue = Mathf.FloorToInt(ui_manager.LODSlider.maxValue - (ui_manager.LODSlider.maxValue * t));
            if(lodValue != latestLODValue)
            {
                latestLODValue = lodValue;
                ui_manager.LODSlider.value = lodValue;

                RemapAndApplyToStepSlider(lodValue, 0, 0, ui_manager.stepSlider.SliderStepDivisions, 1);
                //float newStepSliderValue = Remap(lodValue, 0, 0, ui_manager.stepSlider.SliderStepDivisions, 1);
                
                FindObjectOfType<Reader>().SplitTreeToLOD(lodValue);
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
