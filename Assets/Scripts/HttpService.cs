using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class HttpService : MonoBehaviour
{
    protected string targetUrl = "https://mercury-be-network.herokuapp.com/api/";
    //protected string targetUrl = "http://localhost:8000/api/";
    public string result = "";

    #region Singleton
    private static HttpService instance;
    public static HttpService Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new HttpService();
            }
            return instance;
        }
    }
    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        instance = this;
    }

    #endregion

    #region Http Requests

    public void Post(string title, Dictionary<string, object> data) => StartCoroutine(PostCorutine(title, data));
    private IEnumerator PostCorutine(string title, Dictionary<string, object> data)
    {
        string jsonString = JsonConvert.SerializeObject(data);
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

        UnityWebRequest uwr = new UnityWebRequest(targetUrl + title, "POST");
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("cache-control", "no-cache");

        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            result = uwr.error;
        else
            result = uwr.downloadHandler.text;
    }

    public void Patch(string title, Dictionary<string, object> data) => StartCoroutine(PatchCorutine(title, data));
    private IEnumerator PatchCorutine(string title, Dictionary<string, object> data)
    {
        string jsonString = JsonConvert.SerializeObject(data);
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

        UnityWebRequest uwr = new UnityWebRequest(targetUrl + title, "PATCH");
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("cache-control", "no-cache");
        
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            result = uwr.error;
        else
            result = uwr.downloadHandler.text;
    }

    public void Get(string title, Dictionary<string, string> queryParams) => StartCoroutine(GetCorutine(title, queryParams));
    private IEnumerator GetCorutine(string title, Dictionary<string, string> queryParams)
    {
        WebClient webClient = new WebClient();
        string url = targetUrl + title + "?";
        foreach (var item in queryParams)
        {
            url += item.Key + '=' + item.Value + '&';
        }
        url = url.Remove(url.Length - 1);
        result = "";
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                result = "Connection error, check your internet";
            else
                result = uwr.downloadHandler.text;
        }
    }

    #endregion

    #region Public Methods

    public Dictionary<string, object> GetParsedResult()
    {
        Dictionary<string, object> parsedResult = MiniJSON.Json.Deserialize(result) as Dictionary<string, object>;

        return parsedResult;
    }

    #endregion
}
