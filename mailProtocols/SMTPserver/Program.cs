using System.Net;
using System.Net.Sockets;
using System.Text;

class SmtpServer
{
    static void Main(string[] args)
    {
        // Local IP adress and port 1025
        IPEndPoint iep_smtp = new IPEndPoint(IPAddress.Loopback, 1025);
        Console.WriteLine("SMTP endpoint : " + iep_smtp);

        StartSMTPserver(iep_smtp);

    }

    private static void StartSMTPserver(IPEndPoint iep_smtp)
    {
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(iep_smtp);
        server.Listen(1);
        Console.WriteLine("SMTP server started.");
        listenSMTP(server, iep_smtp);
        Console.ReadKey();
        server.Close();


    }

    private static void listenSMTP(Socket server, IPEndPoint iep_smtp)
    {
        while(true)
        {
            Socket client = server.Accept();
            string client_ip = ((IPEndPoint)(client.RemoteEndPoint)).Address.ToString();
            string client_port = ((IPEndPoint)(client.RemoteEndPoint)).Port.ToString();

            client.Send(Encoding.UTF8.GetBytes("220"+ iep_smtp +" ESMTP Postfix\n"));

            string from = "";
            string to = "";
            StringBuilder data = new StringBuilder();

            while(true)
            {
                byte[] buffer = new byte[2048];
                client.Receive(buffer);
                string client_msg = Encoding.UTF8.GetString(buffer);
                Console.WriteLine("[{0}:{1}]: {2}",client_ip,client_port,client_msg);
                string[] lines = client_msg.Split(
                    new[] { "\r\n", "\r", "\n" },
                        StringSplitOptions.None
                );
                foreach (string line in lines)
                {
                    if (line.StartsWith("HELO"))
                    {
                        client.Send(Encoding.UTF8.GetBytes("250 Hello " +client_ip+", I am glad to meet you\r\n"));
                    }
                    else if (line.StartsWith("MAIL FROM:"))
                    {
                        from = line.Substring("MAIL FROM:".Length).Trim();
                        client.Send(Encoding.UTF8.GetBytes("250 OK\r\n"));
                    }
                    else if (line.StartsWith("RCPT TO:"))
                    {
                        to = line.Substring("RCPT TO:".Length).Trim();
                        client.Send(Encoding.UTF8.GetBytes("250 OK\r\n"));
                    }
                    else if (line.ToUpper() == "DATA")
                    {
                        client.Send(Encoding.UTF8.GetBytes("354 End data with <CRLF>.<CRLF>\r\n"));
                        while (true)
                        {
                            byte[] dataBuffer = new byte[2048];
                            int bytesRead = client.Receive(dataBuffer);

                            if (bytesRead == 0)
                            {
                                // Connection closed by the client
                                client.Close();
                                break;
                            }

                            string dataLine = Encoding.UTF8.GetString(dataBuffer, 0, bytesRead);
                            data.Append(dataLine);
                            Console.WriteLine(dataLine);

                            if (dataLine == ".\r\n")
                            {
                                client.Send(Encoding.UTF8.GetBytes("250 OK: queued as 12345\r\n"));
                                break;
                            }
                        }
                    }
                    else if (line.ToUpper() == "QUIT")
                    {
                        client.Send(Encoding.UTF8.GetBytes("221 Bye\r\n"));
                        break;
                    }
                }
            }
        }
    }

}

