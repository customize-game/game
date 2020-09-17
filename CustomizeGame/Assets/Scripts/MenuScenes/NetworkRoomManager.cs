using CustomizeGame.ServerShared.Hubs.Hubs;
using CustomizeGame.ServerShared.MessagePackObjects;
using Grpc.Core;
using MagicOnion.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

public class NetworkRoomManager : MonoBehaviour, IGameHubReceiver
{

    private Channel channel;
    IGameHub gameHub;
    [SerializeField] private string roomName = default;

    [SerializeField] Text host = default;
    [SerializeField] Text port = default;

    [SerializeField] Text roomMemberDisplay = default;

    [SerializeField] private PlayerMPO self = default;
    [SerializeField] private Dictionary<string, PlayerMPO> players = default;

    [SerializeField] Material defaultMaterial = default;

    //chat Managed UniRx
    private Subject<string> messageSubject;
    [SerializeField] ChatManager chatManager = default;

    [SerializeField] Button trainingMatchButton = default;
    [SerializeField] Button pvpMatchButton = default;
    [SerializeField] Button pveMatchButton = default;

    private bool matchingFlag; 

    public NetworkRoomManager()
    {
        messageSubject = new Subject<string>();
        players = new Dictionary<string, PlayerMPO>();

        matchingFlag = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        //Self PlayerObject Init
        self = new PlayerMPO
        {
            Name = "myRobbot:" + Guid.NewGuid().ToString("N"),

            Transform = new TransformMPO()
            {
                Position = new Vector3(0, 0, 0),
                Rotation = new Quaternion(0, 0, 0, 0)
            },

            Character = new CharacterMPO()
            {
                CharacterModel = GameObject.FindGameObjectWithTag("CharacterModel").name,
                ColorMaterial = defaultMaterial.name
            }
        };

        //自分の入力したチャットイベント購読登録
        //送信イベント受信したらチャット送信
        //※UniRx標準のイベントチェックとかあるっぽいのでそっちでも良いかも
        chatManager.OnReceivedChatSelfMessage.Subscribe(
                message => {
                    sendMessage(message);
                }
            )
            .AddTo(this);

        host.text = "";
        port.text = "";

        //マッチボタン処理のイベント登録　UniRx
        MatchButtonAction();
    }

    #region サーバーからのイベント受信処理

    public void ConnectChannel()
    {
        string target = $"{host.text}:{port.text}";
        Debug.Log("接続先 " + target);
        channel = new Channel(target, ChannelCredentials.Insecure);

        try
        {
            gameHub = StreamingHubClient.Connect<IGameHub, IGameHubReceiver>(this.channel, this);
            JoinSelf();
        }
        catch (System.Exception e)
        {
            Debug.Log("サーバーに接続できませんでした。");
            Debug.LogError(e);
        }
    }

    async void OnDestroy()
    {
        if (gameHub != null)
        {
            Debug.Log($"Destroy {channel.Target}");
            await gameHub.LeaveAsync();
            await gameHub.DisposeAsync();
            if (players.Count == 0)
            {
                await channel.ShutdownAsync();
            }
        }
    }

    public void OnJoin(PlayerMPO otherPlayer)
    {
        string otherPlayerName = otherPlayer.Name;
        if (!otherPlayerName.Equals(self.Name))
        {
            Debug.Log($"{otherPlayerName}さんが入室しました");
            players.Add(otherPlayerName, otherPlayer);
        }
        MemberDisplay();
    }

    public void OnLeave(string name)
    {
        players.Remove(name);
        Debug.Log($"{name}さんが退室しました");
        MemberDisplay();
    }

    public void OnSendMessage(string name, string message)
    {
        string msg = $"{name}:{message}";
        Debug.Log(msg);
        //UniRx event発行
        messageSubject.OnNext(msg);
    }

    public void OnMovePosition(string name, TransformMPO transform)
    {
        if (!name.Equals(self.Name))
        {
            players[name].Transform = transform;
            Debug.Log($"{name}さんが移動しました");
        }
    }


    #endregion

    #region なんかいろいろこの中でやりたい処理

    private void InteractableMatchButton(Button clickedButton) {
        trainingMatchButton.interactable = matchingFlag;
        pvpMatchButton.interactable = matchingFlag;
        pveMatchButton.interactable = matchingFlag;

        if (!matchingFlag) {
            clickedButton.GetComponentInChildren<Text>().text = "Cancel";
            ConnectChannel();
        } else {
            clickedButton.GetComponentInChildren<Text>().text = "Match";
            LeaveSelf();
        }

        matchingFlag = !matchingFlag;
        clickedButton.interactable = true;
    }

    private void MemberDisplay() {
        string displayMsg = "";
        int cnt = 0;
        foreach (String playerName in players.Keys) {
            displayMsg += $"[{cnt}]{playerName.Split(':')[0]}\n";
        }
        roomMemberDisplay.text = displayMsg;
    }

    private void MatchButtonAction() {
        //マッチボタン処理

        trainingMatchButton.onClick.AsObservable()
            .Where(_ => !host.text.Equals(""))
            .Where(_ => !port.text.Equals(""))
            .Subscribe(
               _ => {
                   roomName = "Training";
                   InteractableMatchButton(trainingMatchButton);
                   Debug.Log("Training Maching...");

               })
            .AddTo(this);

        pvpMatchButton.onClick.AsObservable()
            .Where(_ => !host.text.Equals(""))
            .Where(_ => !port.text.Equals(""))
            .Subscribe(
               _ => {
                   roomName = "PvP";
                   InteractableMatchButton(pvpMatchButton);
                   Debug.Log("PvP Maching...");
               })
            .AddTo(this);

        pveMatchButton.onClick.AsObservable()
            .Where(_ => !host.text.Equals(""))
            .Where(_ => !port.text.Equals(""))
            .Subscribe(
               _ => {
                   roomName = "PvE";
                   InteractableMatchButton(pveMatchButton);
                   Debug.Log("PvE Maching...");
               })
            .AddTo(this);
    }


    #endregion

    #region サーバーへの通知処理
    /// <summary>
    /// ルーム入室処理
    /// </summary>
    async void JoinSelf()
    {

        PlayerMPO[] playerArray = await gameHub.JoinAsync(roomName, self);

        foreach (PlayerMPO playerTmp in playerArray)
        {
            if (!playerTmp.Name.Equals(self.Name))
            {
                if (!players.ContainsKey(playerTmp.Name))
                {
                    players.Add(playerTmp.Name, playerTmp);
                }
            }
        }
        MemberDisplay();
    }

    async void LeaveSelf() {
        await gameHub.LeaveAsync();
        await gameHub.DisposeAsync();
        if (players.Count == 0)
        {
            await channel.ShutdownAsync();
        }
    }

    /// <summary>
    /// サーバーへチャットメッセージの送信依頼
    /// </summary>
    /// <param name="message"></param>
    private async void sendMessage(string message) {
        await gameHub.SendMessageAsync(message);
    }

    #endregion

    #region UniRx の公開するイベント購読処理

    /// <summary>
    /// チャットメッセージ購読側 UniRx
    /// </summary>
    public IObservable<string> OnReceivedChatMessage
    {
        get { return messageSubject; }
    }

    #endregion
}
