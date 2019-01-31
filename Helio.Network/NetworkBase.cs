
using Lidgren.Network;
using System;
using System.Collections.Generic;

namespace Helio.Network
{
    public abstract class NetworkBase
    {
        #region Properties

        public NetPeerStatistics NetworkConnectionStats
        {
            get
            {
                return this.NetPeer.Statistics;
            }
        }

        public virtual NetPeer NetPeer
        {
            get
            {
                throw new NotImplementedException("Must be overridden by Client and Server based on their specific implementations.");
            }
        }
        /// <summary>
        /// Must match between Client and Server.
        /// </summary>
        protected string ApplicationName { get; set; }

        /// <summary>
        /// Must match between Client and Server.
        /// </summary>
        protected int Port { get; set; }

        protected virtual NetPeerConfiguration Configuration
        {
            get
            {
                // NOTE: these settings are common between client and server
                return new NetPeerConfiguration(this.ApplicationName)
                {
                    // disconnects after 9 sec
                    ConnectionTimeout = 9.0f,

                    // checks for disconnect every 3 sec                               
                    PingInterval = 3.0f,

                    #region Testing
                    // These two properties control simulating delay of packets in seconds (not milliseconds, use 0.05 for 50 ms of lag). 
                    // They work on top of the actual network delay and the total delay will be: Actual 
                    // one way latency + SimulatedMinimumLatency + (randomly per packet 0 to SimulatedRandomLatency seconds)

                    // TESTING RESULT: causes delay in input... but no real "jumpiness"
                    //SimulatedMinimumLatency = 0.150f, // 150ms of lag

                    // TESTING RESULT: causes random "jumpiness"
                    //SimulatedRandomLatency = 0.025f, // 25ms of random lag //0.100f, // 100ms of random lag

                    // This is a float which determines the chance that a packet will be duplicated at the destination. 0 means no packets 
                    // will be duplicated, 0.5f means that on average, every other packet will be duplicated.
                    // TESTING RESULT: causes random "jumpiness"
                    //SimulatedDuplicatesChance = 0.25f, // 25% of packets lost

                    // This is a float which simulates lost packets. A value of 0 will disable this feature, a value of 0.5f will make 
                    // half of your sent packets disappear, chosen randomly. Note that packets may contain several messages - this is the amount of packets lost.
                    // 25% is too strict. Literally get logged out. Can't control ship effectively.
                    // TESTING RESULT: No issues... seems just fine.
                    //SimulatedLoss = 0.10f, // 10% loss
                    #endregion
                };
            }
        }

        #endregion

        #region Constructors

        public NetworkBase(string applicationName, int port)
        {
            this.ApplicationName = applicationName;
            this.Port = port;
        }

        #endregion

        #region Methods

        /// <summary>
        /// REFERENCE:
        ///     https://github.com/lidgren/lidgren-network-gen3/wiki/Basics
        ///     https://groups.google.com/forum/#!topic/lidgren-network-gen3/Mzim7gqZwv8
        ///     https://github.com/klutch/SFMLFarseerNetwork/blob/master/SFML_Farseer_Network/Managers/NetManager.cs
        /// </summary>
        public void ProcessIncomingMessages()
        {
            NetIncomingMessage incomingMessage;
            while ((incomingMessage = this.NetPeer.ReadMessage()) != null)
            {
                switch (incomingMessage.MessageType)
                {
                    case NetIncomingMessageType.Data:

                        // Determine the message type. 
                        int messageType = incomingMessage.ReadInt32();

                        // Retrieve message data (serialized model).
                        var messageData = incomingMessage.ReadRemainingBytes();

                        // Derive network message
                        var message = new ReceivedNetworkMessage(
                            messageType,
                            messageData,
                            incomingMessage.SenderConnection.RemoteUniqueIdentifier);

                        // get handler
                        var handler = this.MessageHandlers[messageType];

                        // pass to handler
                        handler?.Invoke(message);
                       
                        //// https://groups.google.com/forum/#!topic/lidgren-network-gen3/zf7-rfO14fo
                        //// var timeDelaySinceSent = NetTime.Now - serverConnection.GetLocalTime(this.SendNetTime);
                        //// NOTE it's built in with ReadTime and WriteTime, but in my testing it breaks with protobuf: https://groups.google.com/forum/#!topic/lidgren-network-gen3/G1ZGs0HDVNY
                        //// gets the time the sender wrote the time, but converts it to local time.
                        //var sentNetTime = messageWrapper.SentNetTime;
                        //var sentNetLocalTime = incomingMessage.SenderConnection.GetLocalTime(sentNetTime);
                        //var secondsSinceSent = (float)(NetTime.Now - sentNetLocalTime);
                        //messageWrapper.SecondsSinceSent = secondsSinceSent;
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        // For example: connected or disconnected.
                        Console.WriteLine("Connection '"+incomingMessage.SenderConnection.RemoteUniqueIdentifier+"' status changed to '" + incomingMessage.SenderConnection.Status.ToString()+ "'.");
                        this.AnnounceConnectionStatusChanged(incomingMessage.SenderConnection.Status);
                        break; 

                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        // Get the error message and print it to console.
                        string errorMessage = incomingMessage.ReadString();
                        Console.WriteLine(errorMessage);
                        break;
                    default:
                        // Unexpected type. Just print it.
                        Console.WriteLine("Unhandled type: " + incomingMessage.MessageType);
                        break;
                }
            }
        }


        /// <summary>
        /// TODO: Add caching.
        /// </summary>
        /// <returns></returns>
        protected int GetMessageSequenceChannel(int messageType, NetDeliveryMethod deliveryMethod)
        {
            /// MAX: 31
            //  https://github.com/lidgren/lidgren-network-gen3/wiki/Sequence-Channels
            int sequenceChannel = 0;
            switch (deliveryMethod)
            {
                case NetDeliveryMethod.ReliableUnordered:
                case NetDeliveryMethod.Unreliable:
                    // if it's unreliable or unordered, then they may share a sequence channel, as sequence doesn't matter.
                    sequenceChannel = 0;
                    break;

                default:
                    // in all other cases, sequence matters, so we must use a separate channel per message type.
                    sequenceChannel = messageType;
                    break;
            }
            return sequenceChannel;
        }

        protected NetOutgoingMessage GetOutgoingMessage(int messageType, object message)
        {
            // create message
            var outgoingMessage = this.NetPeer.CreateMessage();

            // record sent time
            //messageWrapper.SentNetTime = NetTime.Now;

            // write the message type
            outgoingMessage.Write(messageType);

            // get bytes
            var messageBytes = ProtocolBufferSerializer.Serialize(message);

            // write our package to the message
            outgoingMessage.Write(messageBytes);

            return outgoingMessage;
        }

        #endregion

        #region Events

        public event EventHandler<NetConnectionStatus> OnConnectionStatusChanged;
        private void AnnounceConnectionStatusChanged(NetConnectionStatus newStatus)
        {
            if (this.OnConnectionStatusChanged != null)
                this.OnConnectionStatusChanged(this, newStatus);
        }

        // https://stackoverflow.com/questions/3813261/how-to-store-delegates-in-a-list
        public delegate void NetworkMessageHandler(ReceivedNetworkMessage message);
        public Dictionary<int, NetworkMessageHandler> MessageHandlers = new Dictionary<int, NetworkMessageHandler>();

        #endregion

    }
}
