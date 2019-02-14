
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Helio.Network.Server
{
    public class NetworkServer : NetworkBase
    {
        #region Overrides

        private NetServer NetServer { get; set; }
        public override NetPeer NetPeer 
        {
            get
            {
                return this.NetServer;
            }
        }

        protected override NetPeerConfiguration Configuration
        {
            get
            {
                var connConfig = base.Configuration;
                connConfig.Port = this.Port;
                return connConfig;
            }
        }

        #endregion

        #region Constructors

        public NetworkServer(string applicationName, int port) : base(applicationName, port)
        {
            // Start the server
            this.NetServer = new NetServer(this.Configuration);
            this.NetServer.Start();

            // Output the server IP to the console
            System.Net.IPAddress ipAddress;
            NetUtility.GetMyAddress(out ipAddress);
            Console.WriteLine("Server address: " + ipAddress.ToString());
        }

        #endregion

        #region Methods 
        
        /// <summary>
        /// TODO: store connections in a hashlist for performance.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        private NetConnection GetConnectionById(long connectionId)
        {
            return this.NetPeer.Connections.FirstOrDefault(con => con.RemoteUniqueIdentifier == connectionId);
        }

        /// <summary>
        /// TODO: store connections in a hashlist for performance.
        /// </summary>
        /// <param name="connectionIds"></param>
        /// <returns></returns>
        private IEnumerable<NetConnection> GetConnectionsByIds(IEnumerable<long> connectionIds)
        {
            return this.NetPeer.Connections.Where(con => connectionIds.Contains( con.RemoteUniqueIdentifier ) );
        }

        public void SendToAllClients(int messageType, object message, NetDeliveryMethod deliveryMethod = NetDeliveryMethod.UnreliableSequenced)
        {
            // simply send to everyone
            this.NetServer.SendToAll(
                 this.GetOutgoingMessage(messageType, message),
                 null, // NOTE: passing 'null' here will just broadcast to all, without excluding any 
                 deliveryMethod,
                 this.GetMessageSequenceChannel(messageType, deliveryMethod)
            );
        }

        public void SendToManyClients(IEnumerable<long> connectionIds, int messageType, object message, NetDeliveryMethod deliveryMethod = NetDeliveryMethod.UnreliableSequenced)
        {
            // get recipient connections
            var recipients = this.GetConnectionsByIds(connectionIds);

            // send message to all recipients
            this.NetServer.SendMessage(
                this.GetOutgoingMessage(messageType, message),
                recipients.ToList(),
                deliveryMethod,
                this.GetMessageSequenceChannel(messageType, deliveryMethod)
            );
        }

        public void SendToOneClient(long connectionId, int messageType, object message, NetDeliveryMethod deliveryMethod = NetDeliveryMethod.UnreliableSequenced)
        {
            // get recipient connection
            var recipient = this.GetConnectionById(connectionId);

            // send message
            this.NetServer.SendMessage(
                this.GetOutgoingMessage(messageType, message),
                recipient,
                deliveryMethod,
                this.GetMessageSequenceChannel(messageType, deliveryMethod)
            );
        }

        #endregion
    }
}
