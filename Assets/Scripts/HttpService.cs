using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class HttpService : MonoBehaviour
{
    protected string targetUrl = "https://mercury-48cab-default-rtdb.europe-west1.firebasedatabase.app/";
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
        /*
         * The code below is only for Alpha purposes,
         * reading the target url for http requests from local file.
         * File path: Needs to be in the Execution file folder.
         */
        StreamReader reader = new StreamReader("./target_url.txt");
        targetUrl = reader.ReadToEnd().Trim();
        reader.Close();
    }

    #endregion

    #region Http Requests

    public void Post(string title, Dictionary<string, string> data) => StartCoroutine(PostCorutine(title, data));
    private IEnumerator PostCorutine(string title, Dictionary<string, string> data)
    {
        string jsonString = "{";
        foreach (var item in data)
        {
            jsonString += "\"" + item.Key + "\": \"" + item.Value + "\",";
        }
        jsonString = jsonString.Remove(jsonString.Length - 1);
        jsonString += "}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

        UnityWebRequest uwr = new UnityWebRequest(targetUrl + title + ".json", "POST");
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

    public void Get(string title) => StartCoroutine(GetCorutine(title));
    private IEnumerator GetCorutine(string title)
    {
        result = "";
        using (UnityWebRequest uwr = UnityWebRequest.Get(targetUrl + title + ".json"))
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
