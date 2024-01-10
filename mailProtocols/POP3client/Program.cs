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
        /// Read credentials from the configuration file
        var jsonString = File.ReadAllText("secrets.json");
        Credentials credentials = JsonSerializer.Deserialize<Credentials>(jsonString);

        string pop3Server = "127.0.0.1";
        int pop3Port = 110; // Default POP3 port

        using (TcpClient client = new TcpClient(pop3Server, pop3Port))
        {
            using (Stream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.ASCII))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII))
            {
                // Read the welcome message from the server
                Console.WriteLine(reader.ReadLine());

                // Send USER command
                SendCommand(writer, "USER "+credentials.Username);

                // Read the response for USER command
                Console.WriteLine(reader.ReadLine());

                // Send PASS command
                SendCommand(writer, "PASS "+credentials.Password);

                // Read the response for PASS command
                Console.WriteLine(reader.ReadLine());

                // Send LIST command
                SendCommand(writer, "LIST");

                // Read the response for LIST command
                Console.WriteLine(reader.ReadLine());

                // Send QUIT command
                SendCommand(writer, "QUIT");

                // Read the response for QUIT command
                Console.WriteLine(reader.ReadLine());
            }
        }
    }

    static void SendCommand(StreamWriter writer, string command)
    {
        writer.WriteLine(command);
        writer.Flush();
    }
}

