using Lidgren.Network;
using System;

namespace Helio.Network.Client
{
    public class NetworkClient : NetworkBase
    {
        #region Overrides 

        private NetClient NetClient { get; set; }
        public override NetPeer NetPeer
        {
            get
            {
                return this.NetClient;
            }
        }

        #endregion

        #region Properties

        private string ServerUrl { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverUrl">IPv4 address from notation (xxx.xxx.xxx.xxx) or hostname</param>
        public NetworkClient( string serverUrl, string applicationName, int port ) : base( applicationName, port )
        {
            this.ServerUrl = serverUrl;

            this.Connect();
        }

        #endregion

        #region Methods

        private void Connect()
        {
            // connect! (or re-connect)
            if (this.NetClient == null)
            {
                // create the client 
                this.NetClient = new NetClient(this.Configuration);
                this.NetClient.Start();
            }

            // Connect
            var connectionToServer = this.NetPeer.Connect(host: this.ServerUrl, port: this.Port);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="deliveryMethod">
        /// 
        ///  Unreliable
        ///     No guarantees, except for preventing duplicates.
        ///
        ///  UnreliableSequenced
        ///     Late messages will be dropped if newer ones were already received.
        ///
        ///  ReliableUnordered
        ///     All packages will arrive, but not necessarily in the same order.
        ///
        ///  ReliableSequenced
        ///     All packages will arrive, but late ones will be dropped.
        ///     This means that we will always receive the latest message eventually, but may miss older ones.
        ///
        ///  ReliableOrdered
        ///     All packages will arrive, and they will do so in the same order.
        ///     Unlike all the other methods, here the library will hold back messages until all previous ones are received, before handing them to us.
        ///
        /// Given these possibilities, why would we ever send a message with one of the less reliable methods?
        /// There are two answers to this question.
        /// First, in real-time games there often is a lot of information that will be transmitted over the network that will be useless after even just a split second, and certainly once more up-to-date information is received. This includes for example player positions in most cases.
        /// In fact, with that kind of information, it is not critical that all updates are received, just as long as enough are to allow for interpolation and prediction of smooth movement.
        /// Second, if we insist on all messages arriving, we may cause a lot of unnecessary network usage, which can cause congestion. If we send more than the connection can handle, it will not be able to catch up with the newest packages. The delay between the actual game and the received network messages would grow ever larger until the game becomes unplayable.
        /// If we however only insist on the arrival of critical messages, this is much less likely to happen. Even if a few – or many – less important messages are lost here and there, the important ones are still likely to be received in reasonable time.
        /// Overall, it is thus advisable to choose a delivery method as unreliable as possible, but as reliable as necessary.
        /// 
        /// </param>
        public void SendToServer(int messageType, object message, NetDeliveryMethod deliveryMethod = NetDeliveryMethod.Unreliable)
        {
            // send message
            this.NetClient.SendMessage(
                this.GetOutgoingMessage(messageType, message),
                deliveryMethod,
                this.GetMessageSequenceChannel(messageType, deliveryMethod)
            );
        }

        #endregion
    }
}
