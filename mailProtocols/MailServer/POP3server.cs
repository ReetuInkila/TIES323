using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Pop3Server
{
    private Inbox inbox;
    private int port;

    public Pop3Server(Inbox inbox, int port)
    {
        this.inbox = inbox;
        this.port = port;
    }

    public void Start()
    {
        IPEndPoint iep_pop3 = new IPEndPoint(IPAddress.Loopback, port);
        Console.WriteLine("POP3 endpoint: " + iep_pop3);
        StartPOP3server(iep_pop3);
    }

    private void StartPOP3server(IPEndPoint iep_pop3)
    {
        // Create a socket for the server
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Create a socket for the server
        server.Bind(iep_pop3);
        // Set the server to listen for incoming connections
        server.Listen(1);

        while (true)
        {
            // Accept incoming client connection
            Socket client = server.Accept();
            string client_ip = ((IPEndPoint)(client.RemoteEndPoint)).Address.ToString();
            string client_port = ((IPEndPoint)(client.RemoteEndPoint)).Port.ToString();

            // Send initial greeting to the client
            client.Send(Encoding.UTF8.GetBytes("+OK POP3 server ready\n"));

            bool conversation = true;

            while (conversation)
            {
                // Receive client messages
                byte[] buffer = new byte[2048];
                int bytesRead = client.Receive(buffer);

                // Check if the connection is closed by the client
                if (bytesRead == 0)
                {
                    // Connection closed by the client
                    client.Close();
                    break;
                }

                // Convert received bytes to string
                string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("[{0}:{1}]: {2}", client_ip, client_port, msg);

                // Split client message into commands
                string[] commands = msg.Split(
                    new[] { " ", "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );

                switch (commands[0].ToUpper())
                {
                    case "USER":
                        client.Send(Encoding.UTF8.GetBytes("+OK User accepted\r\n"));
                        break;
                    case "PASS":
                        client.Send(Encoding.UTF8.GetBytes("+OK Password accepted\r\n"));
                        break;
                    case "LIST":
                        foreach (string mail in inbox.GetMail())
                        {
                            client.Send(Encoding.UTF8.GetBytes(mail));
                        }
                        break;
                    case "QUIT":
                        client.Send(Encoding.UTF8.GetBytes("+OK Bye\r\n"));
                        conversation = false;  // Optionally, you can set this to true if you want to keep the conversation open
                        break;
                    default:
                        client.Send(Encoding.UTF8.GetBytes("-ERR Unknown command\r\n"));
                        break;
                }
            }

            client.Close();
        }
    }
}
