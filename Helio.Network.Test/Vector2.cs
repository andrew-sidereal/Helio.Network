using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Helio.Network.Test
{
    [ProtoContract]
    public class Vector2
    {
        [ProtoMember(1)]
        public float X { get; set; }
        [ProtoMember(2)]
        public float Y { get; set; }

        public override string ToString()
        {
            return string.Format("x:{0},y:{1}", X, Y);
        }
    }
}
