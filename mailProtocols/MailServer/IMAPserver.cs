using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class ImapServer
{
    private Inbox inbox;
    private int port;

    public ImapServer(Inbox inbox, int port)
    {
        this.inbox = inbox;
        this.port = port;
    }

    public void Start()
    {
        IPEndPoint iep_imap = new IPEndPoint(IPAddress.Loopback, port);
        Console.WriteLine("IMAP endpoint: " + iep_imap);
        StartIMAPserver(iep_imap);
    }

    private void StartIMAPserver(IPEndPoint iep_imap)
    {
        // Create a socket for the server
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Create a socket for the server
        server.Bind(iep_imap);
        // Set the server to listen for incoming connections
        server.Listen(1);

        while (true)
        {
            // Accept incoming client connection
            Socket client = server.Accept();
            string client_ip = ((IPEndPoint)(client.RemoteEndPoint)).Address.ToString();
            string client_port = ((IPEndPoint)(client.RemoteEndPoint)).Port.ToString();

            // Send initial greeting to the client
            client.Send(Encoding.UTF8.GetBytes("* OK IMAP server ready for requests from "+client_ip+":"+client_port+"\n"));

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

                switch (commands[1].ToUpper())
                {
                    case "LOGIN":
                        client.Send(Encoding.UTF8.GetBytes(commands[0]+" OK "+ commands[2]+" authenticated (Success)\r\n"));
                        break;
                    case "LIST":
                        List<string> mailList = inbox.GetMail();
                        int size = 0;
                        foreach (string mail in mailList)
                        {
                            size += mail.Length * sizeof(char);
                        }
                        
                        string response = string.Format("+OK {0} messages ({1} octets)\r\n", mailList.Count, size);
                        client.Send(Encoding.UTF8.GetBytes(response));

                        // Now, you might want to send the list of messages with their sizes as well
                        foreach (string mail in mailList)
                        {
                            // Assuming you want to include the size of each message in the response
                            string messageResponse = string.Format("{0} {1}\r\n", mailList.IndexOf(mail) + 1, mail.Length * sizeof(char));
                            client.Send(Encoding.UTF8.GetBytes(messageResponse));
                        }

                        // End the list with a single dot
                        client.Send(Encoding.UTF8.GetBytes(".\r\n"));
                        break;
                    case "LOGOUT":
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
