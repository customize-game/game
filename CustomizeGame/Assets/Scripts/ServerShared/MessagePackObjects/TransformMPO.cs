using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace CustomizeGame.ServerShared.MessagePackObjects
{
    [MessagePackObject]
    public class TransformMPO
    {
        [Key(0)]
        public Vector3 Position { get; set; }

        [Key(1)]
        public Quaternion Rotation { get; set; }
    }
}