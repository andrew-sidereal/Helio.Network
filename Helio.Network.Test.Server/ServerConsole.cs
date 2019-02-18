using Helio.Network.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helio.Network.Test.Server
{
    public class ServerConsole
    {
        private NetworkServer NetworkServer {get; set;}

        public ServerConsole()
        {
            // start server
            this.NetworkServer = new NetworkServer("helio-test", 5125);

            // wire up message handlers
            this.NetworkServer.MessageHandlers.Add((int)TestMessageTypes.Foo, this.FooMessageHandler);
            this.NetworkServer.MessageHandlers.Add((int)TestMessageTypes.Bar, this.BarMessageHandler);

            // start the game loop
            this.Run();
        }

        public void Run()
        {
            while (true)
            {
                // process incoming messages 
                NetworkServer.ProcessIncomingMessages();
            }
        }

        private void FooMessageHandler(ReceivedNetworkMessage message)
        {
            // deserialize
            var foo = message.As<FooMessage>(); 

            // write to console
            Console.WriteLine(
                "Echoing (back to sender) message received from client connection '" + message.SenderConnectionId + 
                "' of type '" + message.MessageType.ToString() + "': " + foo.ToString());

            // echo it back to the sender
            this.NetworkServer.SendToOneClient(message.SenderConnectionId, message.MessageType, foo);
        }

        private void BarMessageHandler(ReceivedNetworkMessage message)
        {
            // deserialize
            var bar = message.As<BarMessage>();

            // write to console
            Console.WriteLine(
                "Echoing (back to sender) message received from client connection '" + message.SenderConnectionId + "' of type '" + message.MessageType.ToString() + "': " + bar.ToString());

            // echo it back to the sender
            this.NetworkServer.SendToOneClient(message.SenderConnectionId, message.MessageType, bar);
        }
    }
}
