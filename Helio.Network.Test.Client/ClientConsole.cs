using Helio.Network.Client;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helio.Network.Test.Client
{
    public class ClientConsole
    {
        private NetworkClient NetworkClient {get; set;}

        public ClientConsole()
        {
            // connect to server
            this.NetworkClient = new NetworkClient("localhost", "helio-test", 5125);

            // wire up message handlers
            this.NetworkClient.MessageHandlers.Add((int)TestMessageTypes.Foo, this.FooMessageHandler);
            this.NetworkClient.MessageHandlers.Add((int)TestMessageTypes.Bar, this.BarMessageHandler);

            // wire up other events
            this.NetworkClient.OnConnectionConnected += NetworkClient_OnConnectionConnected; 

            // start the game loop
            this.Run();
        }

        public void Run()
        {
            Console.WriteLine("Press 'enter' to send a test message and have the server echo it back.");

            while (true)
            {
                // process incoming messages 
                NetworkClient.ProcessIncomingMessages();

                this.ProcessInput();
            }
        }

        private void ProcessInput()
        {
            // Begin with processing input
            if (System.Console.KeyAvailable)
            {
                // We have input, process accordingly
                var userKey = System.Console.ReadKey();

                switch (userKey.Key)
                {
                    case ConsoleKey.Enter:

                        // create test message
                        var foo = new FooMessage();
                        foo.FloatX = 12.98f;
                        foo.IntY = 3985;
                        foo.Name = "This isn't not a test!";
                        foo.Birthday = DateTime.Now;
                        foo.Vector = new Vector2 { X = -9.1f, Y = 88.7f };

                        // for testing, print what it is
                        Console.WriteLine("Sending to server: '" + foo.ToString() + "'");

                        // send test message to server
                        this.NetworkClient.SendToServer((int)TestMessageTypes.Foo, foo);

                        break;

                    case ConsoleKey.Spacebar:

                        // create test message
                        var bar = new BarMessage();
                        bar.TimeOriginallySent = DateTime.Now;

                        // for testing, print what it is
                        Console.WriteLine("Sending to server: '" + bar.ToString() + "'");

                        // send test message to server
                        this.NetworkClient.SendToServer((int)TestMessageTypes.Bar, bar);

                        break;
                }
            }
        }

        private void FooMessageHandler(ReceivedNetworkMessage message)
        {
            // deserialize
            var foo = message.As<FooMessage>();

            // write to console
            Console.WriteLine("Message received from server of type '" + message.MessageType.ToString() + "': " + foo.ToString());
        }

        private void BarMessageHandler(ReceivedNetworkMessage message)
        {
            // deserialize
            var bar = message.As<BarMessage>();

            // write to console
            Console.WriteLine(
                "Message received from client connection '" + message.SenderConnectionId + "' of type '" + message.MessageType.ToString() + "': " + bar.ToString());
        }

        private void NetworkClient_OnConnectionConnected(object sender, NetConnection e)
        {
            Console.WriteLine("You are now connected. Press enter twice to send a test model to the server and have it echoed back.");
        }
    }
}
