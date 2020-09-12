using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace CustomizeGame.ServerShared.MessagePackObjects
{
    [MessagePackObject]
    public class CharacterMPO
    {
        [Key(0)]
        public string CharacterName { get; set; }

        [Key(1)]
        public string CharacterModel { get; set; }

        [Key(2)]
        public string ColorMaterial { get; set; }

    }
}
