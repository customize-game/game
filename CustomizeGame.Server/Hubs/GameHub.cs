using MagicOnion.Server.Hubs;
using CustomizeGame.ServerShared.Hubs.Hubs;
using CustomizeGame.ServerShared.MessagePackObjects;
using System;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityGameServerMO01.Hubs
{
    class GameHub : StreamingHubBase<IGameHub, IGameHubReceiver>, IGameHub
    {
        IGroup room;
        PlayerMPO self;
        private IInMemoryStorage<PlayerMPO> storage;

        public async Task<PlayerMPO[]> JoinAsync(string roomName, PlayerMPO player)
        {
            Console.Out.WriteLine($"JoinAsync:{roomName},{player.Name}");

            //自分の情報を保持
            self = player;

            //ルームに参加&ルームを保持
            (room, storage) = await Group.AddAsync(roomName, self);

            //参加したことをルームに参加している全メンバーに通知
            this.Broadcast(room).OnJoin(self);

            return storage.AllValues.ToArray();
        }

        public async Task LeaveAsync()
        {
            Console.Out.WriteLine($"LeaveAsync:{self.Name}");
            //ルーム内のメンバーから自分を削除
            await room.RemoveAsync(this.Context);
            //退室したことを全メンバーに通知
            this.Broadcast(room).OnLeave(self.Name);
        }

        public async Task SendMessageAsync(string message)
        {
            Console.Out.WriteLine($"SendMessageAsync:{self.Name},{message}");
            //発言した内容を全メンバーに通知
            this.Broadcast(room).OnSendMessage(self.Name, message);

            await Task.CompletedTask;
        }

        public async Task MovePositionAsync(Vector3 position)
        {
            Console.Out.WriteLine($"MovePositionAsync:{self.Name}");
            // サーバー上の情報を更新
            self.Transform.Position = position;

            //更新したプレイヤーの情報を全メンバーに通知
            this.Broadcast(room).OnMovePosition(self.Name, self.Transform);

            await Task.CompletedTask;
        }

        public async Task MoveRotationAsync(Quaternion rotation) {
            self.Transform.Rotation = rotation;

            this.Broadcast(room).OnMovePosition(self.Name, self.Transform);

            await Task.CompletedTask;
        }

        public async Task MoveTransformAsync(TransformMPO transform) {
            self.Transform = transform;

            this.Broadcast(room).OnMovePosition(self.Name, self.Transform);

            await Task.CompletedTask;
        }

        protected override ValueTask OnConnecting()
        {
            // handle connection if needed.
            Console.WriteLine($"client connected {this.Context.ContextId}");
            return CompletedTask;
        }

        protected override ValueTask OnDisconnected()
        {
            Console.Out.WriteLine($"Disconnected:{self.Name}");
            // handle disconnection if needed.
            // on disconnecting, if automatically removed this connection from group.
            return CompletedTask;
        }
    }
}
