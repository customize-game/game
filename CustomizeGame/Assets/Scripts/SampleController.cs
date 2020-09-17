using Grpc.Core;
using MagicOnion.Client;
using CustomizeGame.ServerShared.Hubs;
using CustomizeGame.ServerShared.MessagePackObjects;
using CustomizeGame.ServerShared.Services;
using UnityEngine;

public class SampleController : MonoBehaviour, ISampleHubReceiver
{
    private Channel channel;
    private ISampleService sampleService;
    private ISampleHub sampleHub;

    void Start()
    {
        this.channel = new Channel("localhost:12345", ChannelCredentials.Insecure);
        this.sampleService = MagicOnionClient.Create<ISampleService>(channel);
        this.sampleHub = StreamingHubClient.Connect<ISampleHub, ISampleHubReceiver>(this.channel, this);

        this.SampleServiceTest(1, 2);

        this.SampleHubTest();
    }

    async void OnDestroy()
    {
        await this.sampleHub.DisposeAsync();
        await this.channel.ShutdownAsync();
    }

    /// <summary>
    /// 普通のAPI通信のテスト用のメソッド
    /// </summary>
    async void SampleServiceTest(int x, int y)
    {
        var sumReuslt = await this.sampleService.SumAsync(x, y);
        Debug.Log($"{nameof(sumReuslt)}: {sumReuslt}");

        var productResult = await this.sampleService.ProductAsync(2, 3);
        Debug.Log($"{nameof(productResult)}: {productResult}");
    }

    /// <summary>
    /// リアルタイム通信のテスト用のメソッド
    /// </summary>
    async void SampleHubTest()
    {
        // 自分のプレイヤー情報を作ってみる
        var player = new Player
        {
            Name = "Minami",
            Position = new Vector3(0, 0, 0),
            Rotation = new Quaternion(0, 0, 0, 0)
        };

        // ゲームに接続する
        await this.sampleHub.JoinAsync(player);

        // チャットで発言してみる
        await this.sampleHub.SendMessageAsync("こんにちは！");

        // 位置情報を更新してみる
        player.Position = new Vector3(1, 0, 0);
        await this.sampleHub.MovePositionAsync(player.Position);

        // ゲームから切断してみる
        await this.sampleHub.LeaveAsync();
    }

    #region リアルタイム通信でサーバーから呼ばれるメソッド群

    public void OnJoin(string name)
    {
        Debug.Log($"{name}さんが入室しました");
    }

    public void OnLeave(string name)
    {
        Debug.Log($"{name}さんが退室しました");
    }

    public void OnSendMessage(string name, string message)
    {
        Debug.Log($"{name}: {message}");
    }

    public void OnMovePosition(Player player)
    {
        Debug.Log($"{player.Name}さんが移動しました: {{ x: {player.Position.x}, y: {player.Position.y}, z: {player.Position.z} }}");
    }

    #endregion
}