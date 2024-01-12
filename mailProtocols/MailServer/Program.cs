using System;
using System.Threading;

class MailServer
{
    static void Main()
    {
        Inbox inbox = new Inbox();

        Thread thread1 = new Thread(() => StartSmtpServer(inbox, 1025));
        Thread thread2 = new Thread(() => StartPop3Server(inbox, 1100));

        thread1.Start();
        thread2.Start();
    }

    static void StartSmtpServer(Inbox inbox, int port)
    {
        SmtpServer smtpServer = new SmtpServer(inbox, port);
        smtpServer.Start();
    }

    static void StartPop3Server(Inbox inbox, int port)
    {
        Pop3Server pop3Server = new Pop3Server(inbox, port);
        pop3Server.Start();
    }
}