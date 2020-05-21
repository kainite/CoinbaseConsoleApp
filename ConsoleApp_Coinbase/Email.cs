using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_Coinbase
{
    class Email
    {
        public static void SendEmail(string mailMessage, string mailSubject)
        {
            string eMailAdress = ConfigurationManager.AppSettings["emailAdress"];
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("CoinbaseAlertConsole@gmail.com");
                message.To.Add(new MailAddress(eMailAdress));
                message.Subject = "Coinbase Alert" + mailSubject;
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = mailMessage;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com"; //for gmail host  
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("javardicepraceta@gmail.com", "Javardice@1990");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR Sending Email - " + ex.Message, Console.ForegroundColor);
            }


        }

    }
}
