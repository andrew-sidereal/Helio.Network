using ProtoBuf;
using System;

namespace Helio.Network.Test
{
    [ProtoContract]
    public class FooMessage
    {
        [ProtoMember(1)]
        public float FloatX;

        [ProtoMember(2)]
        public int IntY;

        [ProtoMember(3)]
        public Vector2 NullVector { get; set; }

        [ProtoMember(4)]
        public string Name;
        
        [ProtoMember(5)]
        public Vector2 Vector { get; set; }

        [ProtoMember(6)]
        public DateTime Birthday;


        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4}", this.FloatX, this.IntY, this.Name, this.Birthday, this.Vector);
        }
    }
}
