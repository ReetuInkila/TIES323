using System;
using System.Collections.Generic;

    class Inbox
    {
        private List<string> mail;

        public Inbox()
        {
            mail = new List<string>();
        }

        public void NewMail(string newMail)
        {
            mail.Add(newMail);
        }

        public List<string> GetMail()
        {
            return mail;
        }
    }
