using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Represents a simple SMTP server implementation.
/// </summary>
class SmtpServer
{
    private Inbox inbox;
    private int port;

    public SmtpServer(Inbox inbox, int port)
    {
        this.inbox = inbox;
        this.port = port;
    }

    public void Start()
    {
        IPEndPoint iep_smtp = new IPEndPoint(IPAddress.Loopback, port);
        Console.WriteLine("SMTP endpoint: " + iep_smtp);
        StartSMTPserver(iep_smtp);
    }

    /// <summary>
    /// Starts the SMTP server and listens for incoming client connections.
    /// </summary>
    /// <param name="iep_smtp">The SMTP server's endpoint.</param>
    private void StartSMTPserver(IPEndPoint iep_smtp)
    {
        // Create a socket for the server
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Create a socket for the server
        server.Bind(iep_smtp);

        // Set the server to listen for incoming connections
        server.Listen(1);

        while (true)
        {
            // Accept incoming client connection
            Socket client = server.Accept();
            string client_ip = ((IPEndPoint)(client.RemoteEndPoint)).Address.ToString();
            string client_port = ((IPEndPoint)(client.RemoteEndPoint)).Port.ToString();

            // Send initial greeting to the client
            client.Send(Encoding.UTF8.GetBytes("220 " + iep_smtp + " ESMTP Postfix\n"));

            Boolean conversation = true;
            string from = "";
            string to = "";
            StringBuilder data = new StringBuilder();

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

                // Split client message into lines
                string[] lines = msg.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );

                // Process each line of the client message
                foreach (string line in lines)
                {
                    if (line.StartsWith("HELO"))
                    {
                        client.Send(Encoding.UTF8.GetBytes("250 Hello " + client_ip + ", I am glad to meet you\r\n"));
                    }
                    else if (line.StartsWith("MAIL FROM:"))
                    {
                        // Extract sender email address and respond
                        from = line.Substring("MAIL FROM:".Length).Trim();
                        client.Send(Encoding.UTF8.GetBytes("250 OK\r\n"));
                    }
                    else if (line.StartsWith("RCPT TO:"))
                    {
                        // Extract recipient email address and respond
                        to = line.Substring("RCPT TO:".Length).Trim();
                        client.Send(Encoding.UTF8.GetBytes("250 OK\r\n"));
                    }
                    else if (line.ToUpper() == "DATA")
                    {
                        // Respond to DATA command and process email data
                        client.Send(Encoding.UTF8.GetBytes("354 End data with <CRLF>.<CRLF>\r\n"));
                        while (true)
                        {
                            // Receive email data
                            byte[] dataBuffer = new byte[2048];
                            int dataBytesRead = client.Receive(dataBuffer);

                            // Check if the connection is closed by the client
                            if (dataBytesRead == 0)
                            {
                                // Connection closed by the client
                                client.Close();
                                conversation = false;
                                break;
                            }

                            // Convert received bytes to string
                            string dataLine = Encoding.UTF8.GetString(dataBuffer, 0, dataBytesRead);
                            data.Append(dataLine);

                            // Check for end of email data
                            if (dataLine == ".\r\n")
                            {

                                // Get the email content as a string
                                string emailContent = data.ToString();

                                // Store the email content in the inbox
                                inbox.NewMail(emailContent);

                                // Respond to end of data and simulate email queuing
                                client.Send(Encoding.UTF8.GetBytes("250 OK\r\n"));
                                break;
                            }
                        }
                    }
                    else if (line.ToUpper() == "QUIT")
                    {
                        // Respond to QUIT command and close the connection
                        client.Send(Encoding.UTF8.GetBytes("221 Bye\r\n"));
                        client.Close();
                        conversation = false;
                        break;  // Break from the inner loop, but stay in the outer loop to accept new clients
                    }
                }
            }
        }
    }
}
