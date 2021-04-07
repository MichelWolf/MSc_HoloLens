using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour, IPunObservable
{

    internal UIManager ui_manager;
    public GameObject parentCube;
    public GameObject visualCube;

    public GameObject anchor;

    PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        ui_manager = FindObjectOfType<UIManager>();
        parentCube.SetActive(true);
        visualCube.SetActive(false);

        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
