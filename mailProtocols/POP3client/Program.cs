using System;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using System.Text;
using System.Text.Json;

class Credentials
{
    public string Username { get; set; }
    public string Password { get; set; }
}

class Pop3Client
{
    static void Main(string[] args)
    {
        // Read credentials from the configuration file
        var jsonString = File.ReadAllText("secrets.json");
        Credentials credentials = JsonSerializer.Deserialize<Credentials>(jsonString) ?? new Credentials();

        string pop3Server = "127.0.0.1";
        int pop3Port = 110; 

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            socket.Connect(pop3Server, pop3Port);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Virhe: " + ex.Message);
            Console.ReadKey();
            return;
        }
        NetworkStream ns = new NetworkStream(socket);

        StreamReader reader = new StreamReader(ns);
        StreamWriter writer = new StreamWriter(ns);

        bool user = false;
        bool pswd = false;
        bool transaction = false;
        bool quit = false;

        while(true)
        {
            string msg = reader.ReadLine();
            Console.WriteLine(msg);

            string[] list = msg.Split(' ');

            if(!user && list[0] == "+OK")
            {
                // Send USER command
                SendCommand(writer, "USER " + credentials.Username);
                user = true;
            } 
            else if (user && !pswd && list[0] == "+OK")
            {
                // Send PASS command
                SendCommand(writer, "PASS " + credentials.Password);
                pswd = true;
            } 
            else if (user && pswd && !transaction && list[0] == "+OK")
            {
                // Send LIST command
                SendCommand(writer, "LIST");
                transaction = true;
            }
            else if (transaction && list[0] == ".")
            {
                // Send QUIT command
                SendCommand(writer, "QUIT");
                quit = true;
            }
            else if(quit)
            {
                break;
            }
        }
        writer.Close();
        reader.Close();
        ns.Close();
        socket.Close();

    }

    static void SendCommand(StreamWriter writer, string command)
    {
        writer.WriteLine(command);
        writer.Flush();
    }
}