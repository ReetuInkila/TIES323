using System;
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
        var jsonString = File.ReadAllText("secrets.json");
        Credentials credentials = JsonSerializer.Deserialize<Credentials>(jsonString) ?? new Credentials();

        string imapServer = "127.0.0.1";
        int imapPort = 143; // Port for plain IMAP

        using (var client = new TcpClient(imapServer, imapPort))
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, System.Text.Encoding.ASCII))
        using (var writer = new StreamWriter(stream, System.Text.Encoding.ASCII) { AutoFlush = true })
        {
            HandleServerResponse(reader); // Handle initial server greeting

            // Send LOGIN command
            SendCommand(writer, $"a001 LOGIN {credentials.Username} \"{credentials.Password}\"\r\n");
            HandleServerResponse(reader); // Handle response after LOGIN

            // Additional commands can be added here...

            // Logout
            SendCommand(writer, "a002 LOGOUT\r\n");
            HandleServerResponse(reader); // Handle response after LOGOUT
        }
    }

    static void SendCommand(StreamWriter writer, string command)
    {
        Console.WriteLine(command);
        writer.WriteLine(command);
    }

    static void HandleServerResponse(StreamReader reader)
    {
        string response = reader.ReadLine();
        Console.WriteLine(response);
    }
}
