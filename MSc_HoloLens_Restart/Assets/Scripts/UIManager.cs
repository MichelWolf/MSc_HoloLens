using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    internal NetworkManager network_manager;
    internal HttpFileFetcher http_fetcher;
    internal Reader reader;

    [Tooltip("The Menu of the Application")]
    [SerializeField]
    private GameObject hololensMenu;
    Transform hololensMenuTransform;

    [Tooltip("The Ui Panel to let the user enter name, connect and play")]
    [SerializeField]
    private GameObject controlPanel;

    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    private GameObject progressPanel;

    [Tooltip("The UI Panel of the Lobby")]
    [SerializeField]
    private GameObject lobbyPanel;


    [Tooltip("Input field for the users nickname")]
    [SerializeField]
    private GameObject nameInput;
    TMP_InputField nameInputField;

    [Tooltip("Textfield to list all connected users")]
    [SerializeField]
    private GameObject connectedUsers;
    TextMeshProUGUI connectedUsersText;

    MixedRealityKeyboard keyboard;

    public GameObject serverFileDropdown_obj;
    TMP_Dropdown serverFileDropdown;
    public GameObject localFileDropdown_obj;
    TMP_Dropdown localFileDropdown;
    public GameObject selectedFile_obj;
    TextMeshProUGUI selectedFile_text;
    public GameObject enableAdjustButton;
    public GameObject disableAdjustButton;

    // Start is called before the first frame update
    void Start()
    {
        network_manager = FindObjectOfType<NetworkManager>();
        http_fetcher = FindObjectOfType<HttpFileFetcher>();
        reader = FindObjectOfType<Reader>();

        hololensMenuTransform = hololensMenu.transform;
        keyboard = this.GetComponent<MixedRealityKeyboard>();
        nameInputField = nameInput.GetComponent<TMP_InputField>();
        nameInputField.text = network_manager.defaultName;
        connectedUsersText = connectedUsers.GetComponent<TextMeshProUGUI>();

        serverFileDropdown = serverFileDropdown_obj.GetComponent<TMP_Dropdown>();
        localFileDropdown = localFileDropdown_obj.GetComponent<TMP_Dropdown>();
        selectedFile_text = selectedFile_obj.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        //RotateObjectToCamera(hololensMenuTransform);
        HandleKeyboardInput();
    }

    public void ToogleMenuPanels(bool connectPanelBool, bool progressPanelBool, bool lobbyPanelBool)
    {
        controlPanel.SetActive(connectPanelBool);
        progressPanel.SetActive(progressPanelBool);
        lobbyPanel.SetActive(lobbyPanelBool);
    }

    public void RotateObjectToCamera(Transform rotateThis)
    {
        Vector3 lookAtVector = rotateThis.position + (rotateThis.position - Camera.main.transform.position);
        rotateThis.LookAt(lookAtVector);
    }

    public void HandleKeyboardInput()
    {
        if (keyboard.Visible)
        {
            network_manager.playerName = keyboard.Text;
            nameInputField.text = network_manager.playerName;
        }
    }

    public void OpenKeyboard()
    {
        keyboard.ClearKeyboardText();
        keyboard.ShowKeyboard();
    }

    public void KeyboardCommit()
    {
        keyboard.HideKeyboard();
        keyboard.ClearKeyboardText();
    }

    public void HideKeyboard()
    {
        if (keyboard.Visible)
        {
            keyboard.HideKeyboard();
            keyboard.ClearKeyboardText();
        }
    }

    public void SetConnectedUserText(string connectedUsers)
    {
        connectedUsersText.text = connectedUsers;
    }

    public void SetServerDropdown(List<string> serverFiles)
    {
        serverFileDropdown.ClearOptions();
        serverFileDropdown.AddOptions(serverFiles);
    }

    public void SetLocalFileDropdown(List<string> localFiles)
    {
        localFileDropdown.ClearOptions();
        if (localFiles.Count != 0)
        {
            localFileDropdown.AddOptions(localFiles);
        }
        else
        {
            localFiles.Add("No files found");
            localFileDropdown.AddOptions(localFiles);
        }
    }

    public void ServerDropdownChange()
    {
        http_fetcher.source = HttpFileFetcher.selectedSource.Server;
        reader.fileName = http_fetcher.serverFiles[serverFileDropdown.value];
        selectedFile_text.text = http_fetcher.source.ToString() + ": " + reader.fileName;
        //serverFileDropdown.value
    }

    public void LocalDropdownChange()
    {
        http_fetcher.source = HttpFileFetcher.selectedSource.Local;
        reader.fileName = http_fetcher.localFiles[localFileDropdown.value];

        selectedFile_text.text = http_fetcher.source.ToString() + ": " + reader.fileName;
    }

    public void ToogleAdjustState(bool adjust)
    {
        enableAdjustButton.SetActive(!adjust);
        disableAdjustButton.SetActive(adjust);
    }
}
