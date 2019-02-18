using ProtoBuf;
using System;

namespace Helio.Network.Test
{
    [ProtoContract]
    public class BarMessage
    {
        [ProtoMember(1)]
        public DateTime TimeOriginallySent;


        public override string ToString()
        {
            return string.Format("Message originally sent at: {0}", this.TimeOriginallySent.ToLongTimeString());
        }
    }
}
