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
        listenSMTP(server);
        Console.ReadKey();
        server.Close();


    }

    private static void listenSMTP(Socket server)
    {
        while(true)
        {
            Socket client = server.Accept();
            string client_ip = ((IPEndPoint)(client.RemoteEndPoint)).Address.ToString();
            string client_port = ((IPEndPoint)(client.RemoteEndPoint)).Port.ToString();

            client.Send(Encoding.UTF8.GetBytes("220 TIES323 SMTP server\n"));
            bool conversation = true;

            while(conversation)
            {
                byte[] buffer = new byte[2048];
                client.Receive(buffer);
                string client_msg = Encoding.UTF8.GetString(buffer);
                Console.WriteLine("[{0}:{1}]: {2}",client_ip,client_port,client_msg);
                string[] lines = client_msg.Split(
                    new[] { "\r\n", "\r", "\n" },
                        StringSplitOptions.None
                );
                client.Send(Encoding.UTF8.GetBytes("250 OK\r\n"));
            }
        }
    }

}

