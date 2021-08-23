using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Microsoft.MixedReality.Toolkit.UI;

public class UIManager : MonoBehaviour
{
    internal NetworkManager network_manager;
    internal HttpFileFetcher http_fetcher;
    internal DataManager data_manager;
    internal Reader reader;
    internal PlacementManager placement_manager;

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

    public GameObject hudImageFront;
    public GameObject hudImageBottom;
    public GameObject hudImageRight;

    internal Vector3 lastQueryPos;
    internal float lastQueryRadius;

    public string sourceIDSearchString;
    public TMP_Text sourceIDText;

    public PinchSlider raSlider;
    public PinchSlider decSlider;
    public PinchSlider distanceSlider;
    public PinchSlider rangeSlider;

    public TMP_Text raText;
    public TMP_Text decText;
    public TMP_Text distanceText;
    public TMP_Text rangeText;
    public TMP_Text distanceMaxText;
    public TMP_Text rangeMaxText;



    [Header("Seiten des Hand Menü")]
    public List<GameObject> handMenuPage0;
    public List<GameObject> handMenuPage1;
    public List<GameObject> handMenuPage2;

    // Start is called before the first frame update
    void Start()
    {
        network_manager = FindObjectOfType<NetworkManager>();
        http_fetcher = FindObjectOfType<HttpFileFetcher>();
        data_manager = FindObjectOfType<DataManager>();
        reader = FindObjectOfType<Reader>();
        placement_manager = FindObjectOfType<PlacementManager>();

        hololensMenuTransform = hololensMenu.transform;
        keyboard = this.GetComponent<MixedRealityKeyboard>();
        nameInputField = nameInput.GetComponent<TMP_InputField>();
        nameInputField.text = network_manager.defaultName;
        connectedUsersText = connectedUsers.GetComponent<TextMeshProUGUI>();

        serverFileDropdown = serverFileDropdown_obj.GetComponent<TMP_Dropdown>();
        localFileDropdown = localFileDropdown_obj.GetComponent<TMP_Dropdown>();
        selectedFile_text = selectedFile_obj.GetComponent<TextMeshProUGUI>();
        //LODSlider = LODSliderObj.GetComponent<Slider>();
        SetLegendColor();

        SwitchHandMenu(0);
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

    //public void SetLODSliderMax(int max)
    //{
    //    LODSlider.maxValue = max;

    //    //stepSlider.SliderStepDivisions = max;

    //}

    public void OnLODSliderChange()
    {
        if (data_manager.octree == null)
        {
            return;
        }

        placement_manager.queryObjectArea.SetActive(true);

        float ra = Mathf.Lerp(0, 360, raSlider.SliderValue);
        float dec = Mathf.Lerp(-90, 90, decSlider.SliderValue);
        float distance = Mathf.Lerp(1, 26000, distanceSlider.SliderValue);
        float range = Mathf.Lerp(0.01f, 2f, rangeSlider.SliderValue);

        //Debug.Log(String.Format("Ra: {0} , Dec: {1}, Distance: {2}", ra, dec, distance));

        raText.text = "Rektaszension (ra): " + ra.ToString("F2");
        decText.text = "Deklination (dec): " + dec.ToString("F2");
        distanceText.text = "Distanz (parsec): " + distance.ToString("F2");
        rangeText.text = "Radius (parsec): " + (range * (data_manager.maxDistance / 2f)).ToString("F2");

        distanceMaxText.text = Mathf.RoundToInt(data_manager.maxDistance).ToString();
        rangeMaxText.text = Mathf.RoundToInt(data_manager.maxDistance).ToString();

        placement_manager.PlaceROIWithAngle(ra, dec, distance, range);
    }

    public void DebugText()
    {
        //Vector3 centerPos = placement_manager.visualCube.transform.position + data_manager.octree.rootNode.averagePositionOfNodes;
        //Vector3 edgePos = centerPos + (Vector3.Cross(Camera.main.transform.up, centerPos - Camera.main.transform.position).normalized * (data_manager.octree.rootNode.distanceFromAverage / 2) * placement_manager.visualCube.transform.localScale.x);
        //Vector3 screenCenterPos = Camera.main.WorldToScreenPoint(centerPos);
        if (reader != null)
        {
            if (data_manager.averageSpectralM != null && data_manager.averageSpectralK != null && data_manager.averageSpectralG != null && data_manager.averageSpectralF != null && data_manager.averageSpectralA != null)
                debugText.text = "LOD: " + FindObjectOfType<PlacementManager>().latestLODValue + "\n" +
                    "# Partikel: " + (data_manager.averageSpectralM.Count + data_manager.averageSpectralK.Count + data_manager.averageSpectralG.Count + data_manager.averageSpectralF.Count + data_manager.averageSpectralA.Count) + "\n" +
                    "Distanz: + " + FindObjectOfType<PlacementManager>().dist.ToString("F2"); 
        }
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

    public void DisableLoadDataButton()
    {
        loadDataButton.SetActive(false);
    }

    public void SwitchHandMenu(int page)
    {
        if(page == 0)
        {
            SetHandMenuPage0(true);
            SetHandMenuPage1(false);
            SetHandMenuPage2(false);


        }
        else if (page == 1)
        {
            SetHandMenuPage0(false);
            SetHandMenuPage1(true);
            SetHandMenuPage2(false);
        }
        else if(page == 2)
        {
            SetHandMenuPage0(false);
            SetHandMenuPage1(false);
            SetHandMenuPage2(true);
        }
    }

    public void SetHandMenuPage0(bool value)
    {
        foreach (GameObject obj in handMenuPage0)
        {
            obj.SetActive(value);
        }
    }

    public void SetHandMenuPage1(bool value)
    {
        foreach (GameObject obj in handMenuPage1)
        {
            obj.SetActive(value);
        }
    }

    public void SetHandMenuPage2(bool value)
    {
        foreach (GameObject obj in handMenuPage2)
        {
            obj.SetActive(value);
        }
    }

    public void SetHUDMapPosition(Vector3 pos, float scale)
    {
        Vector3 newPos;
        float newScale;
        if (placement_manager.ROI)
        {
            newPos = lastQueryPos + (pos * lastQueryRadius);
            newScale = lastQueryRadius * scale;
        }
        else
        {
            newPos = pos;
            newScale = scale;
        }


        hudImageFront.GetComponent<RectTransform>().localPosition = new Vector3(newPos.x, newPos.y, 0);
        hudImageBottom.GetComponent<RectTransform>().localPosition = new Vector3(newPos.x, -newPos.z, 0);
        hudImageRight.GetComponent<RectTransform>().localPosition = new Vector3(newPos.z, newPos.y, 0);

        hudImageFront.GetComponent<RectTransform>().localScale = new Vector3(newScale, newScale, 0);
        hudImageBottom.GetComponent<RectTransform>().localScale = new Vector3(newScale, newScale, 0);
        hudImageRight.GetComponent<RectTransform>().localScale = new Vector3(newScale, newScale, 0);


        lastQueryPos = newPos;
        lastQueryRadius = newScale;
    }


    public void ModifySourceIDSearchString(int value)
    {
        if (value >= 0)
        {
            sourceIDSearchString += value.ToString();
        }
        else if (value == -1)
        {
            if (sourceIDSearchString.Length > 0)
            {
                sourceIDSearchString = sourceIDSearchString.Remove(sourceIDSearchString.Length - 1, 1);
            }
        }
        else if (value == -2)
        {
            sourceIDSearchString = "";
        }

        sourceIDText.text = "ID: " + sourceIDSearchString;
    }

}
