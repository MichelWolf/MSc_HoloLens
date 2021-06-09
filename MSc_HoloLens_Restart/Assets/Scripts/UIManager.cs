using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

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
    public GameObject loadDataButton;

    public GameObject LODSliderObj;
    public Slider LODSlider;

    public GameObject galacticCenter;

    public Image spectralImageM;
    public Image spectralImageK;
    public Image spectralImageG;
    public Image spectralImageF;
    public Image spectralImageA;

    public TextMeshPro spectralMText;
    public TextMeshPro spectralKText;
    public TextMeshPro spectralGText;
    public TextMeshPro spectralFText;
    public TextMeshPro spectralAText;

    public TextMeshProUGUI countM;
    public TextMeshProUGUI countK;
    public TextMeshProUGUI countG;
    public TextMeshProUGUI countF;
    public TextMeshProUGUI countA;

    public StepSlider stepSlider;
    public int latestLODValue;

    public TextMeshProUGUI debugText;
    public GameObject hud;

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
        LODSlider = LODSliderObj.GetComponent<Slider>();
        SetLegendColor();
    }

    // Update is called once per frame
    void Update()
    {
        //RotateObjectToCamera(hololensMenuTransform);
        HandleKeyboardInput();
        PlaceHUD();
        DebugText();
    }

    private void PlaceHUD()
    {
        hud.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
        hud.transform.LookAt((hud.transform.position - Camera.main.transform.position) + hud.transform.position);
        hud.transform.rotation = Camera.main.transform.rotation;
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

    public void SetLODSliderMax(int max)
    {
        LODSlider.maxValue = max;

        stepSlider.SliderStepDivisions = max;
        
    }

    public void OnLODSliderChange()
    {
        int lodValue = Mathf.RoundToInt(stepSlider.SliderValue * stepSlider.SliderStepDivisions);


        //int lodValue = Mathf.FloorToInt(ui_manager.LODSlider.maxValue - (ui_manager.LODSlider.maxValue * t));
        if (lodValue != latestLODValue)
        {
            latestLODValue = lodValue;
            //ui_manager.LODSlider.value = lodValue;
            Debug.Log(lodValue);
            //FindObjectOfType<Reader>().SplitTreeToLOD(lodValue, false);
        }


        //reader.SplitTreeToLOD((int)LODSlider.value);
    }

    public void DebugText()
    {
        if (reader != null)
        {
            if (reader.averageSpectralM != null && reader.averageSpectralK != null && reader.averageSpectralG != null && reader.averageSpectralF != null && reader.averageSpectralA != null)
                debugText.text = "LOD Level: " + FindObjectOfType<PlacementManager>().latestLODValue + "\n" +
                    "# Partikel: " + (reader.averageSpectralM.Count + reader.averageSpectralK.Count + reader.averageSpectralG.Count + reader.averageSpectralF.Count + reader.averageSpectralA.Count) + "\n" +
                    "Distanz: + " + FindObjectOfType<PlacementManager>().dist.ToString("F2"); 
        }
    }


    public void IncreaseLOD()
    {
        LODSlider.value += 1;

        stepSlider.SliderValue = 1;
        //reader.SplitTreeToLOD((int)LODSlider.value, false);
    }

    public void DecreaseLOD()
    {
        LODSlider.value -= 1;
        //reader.SplitTreeToLOD((int)LODSlider.value, false);
    }

    public void SetLegendColor()
    {
        SpawnSpheres spawner = FindObjectOfType<SpawnSpheres>();

        spectralMText.color = spawner.spectralColorM;
        spectralKText.color = spawner.spectralColorK;
        spectralGText.color = spawner.spectralColorG;
        spectralFText.color = spawner.spectralColorF;
        spectralAText.color = spawner.spectralColorA;

        spectralImageM.color = spawner.spectralColorM;
        spectralImageK.color = spawner.spectralColorK;
        spectralImageG.color = spawner.spectralColorG;
        spectralImageF.color = spawner.spectralColorF;
        spectralImageA.color = spawner.spectralColorA;
    }

    public void SetLegendCount(int M, int K, int G, int F, int A)
    {
        countM.text = M.ToString();
        countK.text = K.ToString();
        countG.text = G.ToString();
        countF.text = F.ToString();
        countA.text = A.ToString();
    }
}
