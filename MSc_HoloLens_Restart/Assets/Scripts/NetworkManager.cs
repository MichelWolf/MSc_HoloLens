using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class NetworkManager : MonoBehaviourPunCallbacks, IPunObservable
{

    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 8;

    
    [Tooltip("UI Text for Player Name display")]
    [SerializeField]
    private GameObject playerNames;

    public string defaultName = "";
    public string playerName = "";
    
    string gameVersion = "1";
    bool isConnecting;

    Reader reader;
    //public Vector3[] pointCloud;

    internal UIManager ui_manager;
    

    void Start()
    {
        ui_manager = FindObjectOfType<UIManager>();
        ui_manager.ToogleMenuPanels(true, false, false);
        
        playerName = defaultName;
        
        //pointCloud = new Vector3[47286];

        reader = FindObjectOfType<Reader>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Connect()
    {
        ui_manager.ToogleMenuPanels(false, true, false);

        if(playerName == "")
        {
            playerName = defaultName;
        }
        PhotonNetwork.NickName = playerName;
        ui_manager.HideKeyboard();

        isConnecting = true;
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public void ShowPlayerNamesInLobby()
    {
        string players = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            players += player.NickName + "\n";
        }
        ui_manager.SetConnectedUserText(players);
    }

    public override void OnJoinedRoom()
    {
        ui_manager.ToogleMenuPanels(false, false, true);

        ShowPlayerNamesInLobby();
        FindObjectOfType<HttpFileFetcher>().GetData();
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
        // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
        if (isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }

    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        ui_manager.ToogleMenuPanels(true, false, false);

        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        ShowPlayerNamesInLobby();

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        ShowPlayerNamesInLobby();
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        LeaveRoom();
    }
    

    public void Load_kdTree()
    {
        photonView.RPC("RPC_Load_kdTree", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void RPC_Load_kdTree()
    {
        reader.ReadFile();
    }


    public void QueryTree()
    {
        Vector3 pos = FindObjectOfType<SpawnSpheres>().queryPos.transform.localPosition;
        photonView.RPC("RPC_QueryTree", RpcTarget.MasterClient, pos);
    }

    [PunRPC]
    public void RPC_QueryTree(Vector3 position)
    {
        FindObjectOfType<SpawnSpheres>().QueryTree(position);
    }


    //public void SendQueryResult(int index)
    //{
    //    photonView.RPC("RPC_SendQueryResult", RpcTarget.All, index);

    //}

    //[PunRPC]
    //public void RPC_SendQueryResult(int index)
    //{
    //    FindObjectOfType<SpawnSpheres>().debugCube.transform.localPosition = reader.pointCloud[index];
    //}

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if (stream.IsWriting == true)
        //{
        //    for (int i = 0; i < reader.pointCloud.Length; i++)
        //    {
        //        stream.SendNext((Vector3)pointCloud[i]);
        //    }
        //}

        //else
        //{
        //    for (int i = 0; i < pointCloud.Length; i++)
        //    {
        //        pointCloud[i] = (Vector3)stream.ReceiveNext();

        //    }
        //    FindObjectOfType<SpawnSpheres>().ApplyToParticleSystem(pointCloud);
        //}
    }
}
