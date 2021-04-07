using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class HttpFileFetcher : MonoBehaviour
{
    const string fetchURL = "http://tanzwolfmedia.tech/HoloLens_Data/fetchData.php";
    const string dataURL = "http://tanzwolfmedia.tech/HoloLens_Data/Data/";
    public List<string> serverFiles;
    public List<string> localFiles;
    internal UIManager ui_manager;
    public enum selectedSource { Server, Local }
    public selectedSource source = selectedSource.Server;
    // Start is called before the first frame update
    void Start()
    {
        ui_manager = FindObjectOfType<UIManager>();
        serverFiles = new List<string>();
        localFiles = new List<string>();
        ResetLists();
        //string fileName = "parsedValues100.bin";
        //StartCoroutine(FetchDataCoroutine());
        //if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data/" + fileName))
        //{
        //    //StartCoroutine(FetchFileCoroutine(fileName));
        //}

        ////Debug.Log(Environment.SpecialFolder.ApplicationData);
        ////Debug.Log(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        //string[] tmp = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data/");
        //foreach (string s in tmp)
        //{
        //    localFiles.Add(Path.GetFileName(s));
        //    //Debug.Log("File in directory: " + Path.GetFileName(s));
        //}
        //ui_manager.SetLocalFileDropdown(localFiles);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetData()
    {
        ui_manager = FindObjectOfType<UIManager>();
        ResetLists();


        StartCoroutine(FetchDataCoroutine());
        string[] tmp = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data/");
        foreach (string s in tmp)
        {
            localFiles.Add(Path.GetFileName(s));
        }
        ui_manager.SetLocalFileDropdown(localFiles);
    }

    public IEnumerator FetchDataCoroutine()
    {
        WWWForm form = new WWWForm();
        WWW www = new WWW(fetchURL);

        yield return www;

        string result = "";

        if (www.error == null)
        {
            result = www.text;
            //string[] tmp = result.Split(' ');
            //Debug.Log(result);
            serverFiles.AddRange(result.Split(' '));
            //foreach (string s in serverFiles)
            //{
            //    Debug.Log("File on server: " + s);
            //}
            ui_manager.SetServerDropdown(serverFiles);

            //Debug.Log(www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }

    }

    public IEnumerator FetchFileCoroutine(string fileName)
    {
        WebClient myWebClient = new WebClient();

        Uri myStringWebResource = new Uri(dataURL + fileName);

        Debug.Log(string.Format("Downloading File \"{0}\" from \"{1}\" .......\n\n", fileName, myStringWebResource));
        try
        {

            myWebClient.DownloadFileAsync(myStringWebResource, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data/" + fileName);
            //myWebClient.DownloadFileAsync(myStringWebResource, Environment.SpecialFolder.ApplicationData + "/HoloLens Data/" + "parsedValues100.bin");
            //myWebClient.DownloadFileAsync(myStringWebResource, Application.dataPath + "/StreamingAssets/" + "parsedValues100.bin");
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }

        yield return null;
    }

    public async Task FetchFile(string fileName)
    {
        WebClient myWebClient = new WebClient();

        Uri myStringWebResource = new Uri(dataURL + fileName);

        Debug.Log(string.Format("Downloading File \"{0}\" from \"{1}\" .......\n\n", fileName, myStringWebResource));
        try
        {
            Debug.Log("Start Download von " + fileName);
            await Task.WhenAll(myWebClient.DownloadFileTaskAsync(myStringWebResource, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HoloLens Data/" + fileName));
            Debug.Log("Download von " + fileName + " abgeschlossen");
            //myWebClient.DownloadFileAsync(myStringWebResource, Environment.SpecialFolder.ApplicationData + "/HoloLens Data/" + "parsedValues100.bin");
            //myWebClient.DownloadFileAsync(myStringWebResource, Application.dataPath + "/StreamingAssets/" + "parsedValues100.bin");
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    public void ResetLists()
    {
        serverFiles.Clear();
        localFiles.Clear();
    }
}
