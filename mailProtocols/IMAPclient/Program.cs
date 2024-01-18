using System;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;

class Credentials
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

class IMAPClient
{
    static void Main(string[] args)
    {
        // Read credentials from the configuration file
        var jsonString = File.ReadAllText("secrets.json");
        Credentials credentials = JsonSerializer.Deserialize<Credentials>(jsonString) ?? new Credentials();

        string imapServer = "127.0.0.1";
        int imapPort = 143; 

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            socket.Connect(imapServer, imapPort);
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

        int counter = 1;
        string state = "auth";
        bool quit = false;

        while(true)
        {
            string msg = reader.ReadLine();
            Console.WriteLine(msg);

            string[] list = msg.Split(' ');

            if(state == "auth" && list[1] == "OK")
            {
                string command = "a" + counter.ToString("000")+" LOGIN " + credentials.Username + " \"" +credentials.Password+"\"\r";
                SendCommand(writer, command);
                state = "select";
            } 
            else if (state =="select" && msg.StartsWith("a" + counter.ToString("000")+" OK"))
            {
                counter ++;
                SendCommand(writer, "a" + counter.ToString("000")+" select inbox\r");
                state = "fetch";
            }
            else if ( state == "fetch" && msg.StartsWith("a" + counter.ToString("000")+" OK"))
            {
                counter ++;
                SendCommand(writer, "a" + counter.ToString("000") + " FETCH 1:* (UID)\r");
                state = "transaction";
            }
            else if (state == "transaction" && msg.StartsWith("a" + counter.ToString("000")+" OK"))
            {
                // Send QUIT command
                counter ++;
                SendCommand(writer, "a" + counter.ToString("000") + " logout\r");
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
        Console.WriteLine(command);
        writer.WriteLine(command);
        writer.Flush();
    }
}