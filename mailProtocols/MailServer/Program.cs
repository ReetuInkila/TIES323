using System;
using System.Threading.Tasks;


class MailServer
{
    static void Main()
    {
        Inbox inbox = new Inbox();

        Task.Run(() => StartSmtpServer(inbox, 1025));

        //Task.Run(() => StartPop3Server(inbox));

        Console.WriteLine("Server listening ports:\nSMTP    1025\nPOP3    110");
        Console.WriteLine("Press Enter to stop the server.");
        Console.ReadLine();
    }

    static async Task StartSmtpServer(Inbox inbox, int port)
    {
        SmtpServer smtpServer = new SmtpServer(inbox, port);
        smtpServer.Start();
    }
}
