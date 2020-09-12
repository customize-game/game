using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class ChatManager : MonoBehaviour
{
    [SerializeField] NetworkRoomManager networkRoomManager;
    [SerializeField] Text chatField;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] InputField sendMessage;

    //chat Managed UniRx
    private Subject<string> selfMessageSubject;

    ChatManager() {
        selfMessageSubject = new Subject<string>();
    }

    // Start is called before the first frame update
    void Start()
    {
        chatField.text = "";
        sendMessage.text = "";

        //スクロールを一番下にする
        scrollRect.verticalNormalizedPosition = 0;

        //chat Messageの受信イベント　UniRx
        networkRoomManager.OnReceivedChatMessage.Subscribe(
            message => { 
                chatField.text += "\n" + message;
            }
            )
            .AddTo(this);
    }

    public void OnChatMessageSend() {
        string msg = sendMessage.text;

        if (!msg.Equals(""))
        {
            selfMessageSubject.OnNext(msg);
        }
        sendMessage.text = "";
    }

    /// <summary>
    /// チャットメッセージ購読側 UniRx
    /// </summary>
    public IObservable<string> OnReceivedChatSelfMessage
    {
        get { return selfMessageSubject; }
    }
}
