using MagicOnion;
using CustomizeGame.ServerShared.MessagePackObjects;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomizeGame.ServerShared.Hubs
{
    /// <summary>
    /// Client -> ServerのAPI
    /// </summary>
    public interface ISampleHub : IStreamingHub<ISampleHub, ISampleHubReceiver>
    {
        /// <summary>
        /// ゲームに接続することをサーバに伝える
        /// </summary>
        Task JoinAsync(Player player);
        /// <summary>
        /// ゲームから切断することをサーバに伝える
        /// </summary>
        Task LeaveAsync();
        /// <summary>
        /// メッセージをサーバに伝える
        /// </summary>
        Task SendMessageAsync(string message);
        /// <summary>
        /// 移動したことをサーバに伝える
        /// </summary>
        Task MovePositionAsync(Vector3 position);
    }

    /// <summary>
    /// Server -> ClientのAPI
    /// </summary>
    public interface ISampleHubReceiver
    {
        /// <summary>
        /// 誰かがゲームに接続したことをクライアントに伝える
        /// </summary>
        void OnJoin(string name);
        /// <summary>
        /// 誰かがゲームから切断したことをクライアントに伝える
        /// </summary>
        void OnLeave(string name);
        /// <summary>
        /// 誰かが発言した事をクライアントに伝える
        /// </summary>
        void OnSendMessage(string name, string message);
        /// <summary>
        /// 誰かが移動した事をクライアントに伝える
        /// </summary>
        void OnMovePosition(Player player);
    }
}