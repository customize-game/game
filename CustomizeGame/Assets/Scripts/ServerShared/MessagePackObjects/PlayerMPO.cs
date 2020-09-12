using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace CustomizeGame.ServerShared.MessagePackObjects
{

    [MessagePackObject]
    public class PlayerMPO
    {
        [Key(0)]
        public string Name { get; set; }

        [Key(1)]
        public TransformMPO Transform { get; set; }

        [Key(2)]
        public CharacterMPO Character { get; set; }
    }
}