using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempMail.Utilities;

namespace TempMail.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // To get a new temporary email
            var tempMail = new TempMailClient();

            // To get all available domains
            var domains = tempMail.AvailableDomains;

            // To get Mailbox
            var mails = tempMail.GetMails();

            // To change email to a specific login@domain
            tempMail.ChangeEmail("loginexample", domains[0]);

            // To get a new temporary email with a specific login@domain
            tempMail = new TempMailClient("loginexample", domains[0]);

            // To get a new temporary email with with login and random domain
            tempMail = new TempMailClient("loginexample");

            // To delete current email and get a new one
            tempMail.Delete();

            // To get the current email account
            string email = tempMail.Email;
        }


    }
}
