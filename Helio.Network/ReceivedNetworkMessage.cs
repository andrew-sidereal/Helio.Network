using System;
using System.Collections.Generic;
using System.Text;

namespace Helio.Network
{
    public class ReceivedNetworkMessage
    {
        public byte[] Data { get; private set; }
        public int MessageType { get; private set; }
        public long SenderConnectionId { get; private set; } 

        public ReceivedNetworkMessage(int messageType, byte[] data, long senderConnectionId)
        {
            this.MessageType = messageType;
            this.Data = data;
            this.SenderConnectionId = senderConnectionId;
        }

        public T As<T>()
        {
            return ProtocolBufferSerializer.Deserialize<T>(this.Data);
        }
    }
}
