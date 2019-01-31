using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Helio.Network
{
    public static class ProtocolBufferSerializer
    {
        public static T Deserialize<T>(byte[] bytes)
        {
            using (MemoryStream memStream = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(memStream);
            }
        }

        public static byte[] Serialize(object obj)
        {
            if (obj == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
