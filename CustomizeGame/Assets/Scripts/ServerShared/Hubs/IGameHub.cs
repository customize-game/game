using MagicOnion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomizeGame.ServerShared.MessagePackObjects;
using System.Threading.Tasks;

namespace CustomizeGame.ServerShared.Hubs.Hubs
{

    public interface IGameHub : IStreamingHub<IGameHub, IGameHubReceiver>
    {
        /// <summary>
        /// ゲームに接続することをサーバに伝える
        /// </summary>
        Task<PlayerMPO[]> JoinAsync(string roomName, PlayerMPO player);
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
        /// <summary>
        /// 回転したことをサーバーに伝える
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        Task MoveRotationAsync(Quaternion rotation);
        /// <summary>
        /// 位置・回転情報をサーバーに伝える
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        Task MoveTransformAsync(TransformMPO transform);

    }
    public interface IGameHubReceiver
    {
        /// <summary>
        /// 誰かがゲームに接続した時の処理
        /// </summary>
        void OnJoin(PlayerMPO player);
        /// <summary>
        /// 誰かがゲームから切断したときの処理
        /// </summary>
        void OnLeave(string name);
        /// <summary>
        /// 誰かが発言したときの処理
        /// </summary>
        void OnSendMessage(string name, string message);
        /// <summary>
        /// 誰かが移動した時の処理
        /// </summary>
        void OnMovePosition(string name, TransformMPO transform);

    }
}