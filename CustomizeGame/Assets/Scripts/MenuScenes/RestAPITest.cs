using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RestAPITest : MonoBehaviour
{
    public Text url;
    public ToggleGroup methodToggleGroup;
    public Text requestEntity;
    public Text responseEntity;

    public async void OnClickSendRequest() {
        string method = methodToggleGroup.ActiveToggles()
            .FirstOrDefault().gameObject.name;

        string requestUri = url.text;
        string jsonStr = requestEntity.text;

        if (requestUri != null && !requestUri.Equals("")) {
            switch (method) {
                case "GetToggle":
                    await UnityWebRequestGet(requestUri);
                    //UniTask() 使った場合
                    string uniTaskGetRes = await UniTaskRequestGet(requestUri);
                    Debug.Log("UniTaskReq:" + uniTaskGetRes);
                    break;
                case "PostToggle":
                    if (jsonStr != null && !jsonStr.Equals(""))
                    {
                        await UnityWebRequestPost(requestUri, jsonStr);
                        //UniTask() 使った場合
                        string uniTaskPostRes = await UniTaskRequestPost(requestUri, jsonStr);
                        Debug.Log("UniTaskReq:" + uniTaskPostRes);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private async UniTask<string> UniTaskRequestGet(string uri)
    {
        var uwr = UnityWebRequest.Get(uri);

        // SendWebRequestが終わるまでawait 
        await uwr.SendWebRequest();

        if (uwr.isHttpError || uwr.isNetworkError)
        {
            // 失敗していたらそのまま例外をthrow
            throw new Exception(uwr.error);
        }

        return uwr.downloadHandler.text;
    }

    private async UniTask<string> UniTaskRequestPost(string uri, string jsonStr)
    {
        var uwr = UnityWebRequest.Post(uri, jsonStr);

        // SendWebRequestが終わるまでawait 
        await uwr.SendWebRequest();

        if (uwr.isHttpError || uwr.isNetworkError)
        {
            // 失敗していたらそのまま例外をthrow
            throw new Exception(uwr.error);
        }

        return uwr.downloadHandler.text;
    }


    public IEnumerator UnityWebRequestGet(string url)
    {
        Debug.Log("send get request");
        var request = UnityWebRequest.Get(url);
        //request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                responseEntity.text = request.downloadHandler.text;
            }
            else
            {
                Debug.Log("failed");
            }
        }
    }

    public IEnumerator UnityWebRequestPost(string url, string jsonStr)
    {
        Debug.Log("send post request");
        var uwr = UnityWebRequest.Post(url, jsonStr);
        
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            if (uwr.responseCode == 200)
            {
                responseEntity.text = uwr.downloadHandler.text;
            }
            else
            {
                Debug.Log("failed");
            }
        }
    }
}
