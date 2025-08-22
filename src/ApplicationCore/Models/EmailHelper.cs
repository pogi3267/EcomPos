using System;
using System.Net.Mail;

namespace ApplicationCore.Models
{
    public class EmailHelper
    {
        public bool SendEmailPasswordReset(string userEmail, string link)
        {
            string fromMail = "devense.noreply@gmail.com";
            string fromPassword = "qgpfuvewwxgbrctz";


            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromMail);
            mailMessage.Subject = "Password Reset";
            mailMessage.To.Add(new MailAddress(userEmail));

            mailMessage.Body = link;
            mailMessage.IsBodyHtml = true;

            var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential(fromMail, fromPassword),
                EnableSsl = true
            };

            try
            {
                client.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool SendEmailPasswordResetV2(string userEmail, string link)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("saurove@diu.edu.bd");
            mailMessage.To.Add(new MailAddress(userEmail));

            mailMessage.Subject = "Password Reset";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = link;

            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential("saurove@diu.edu.bd", "132752.");
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true; // Enable SSL
            client.UseDefaultCredentials = false; // Disable default credentials

            try
            {
                client.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                // log exception
            }
            return false;
        }

        public bool SendEmailPasswordResetV3(string userEmail, string link)
        {
            try
            {

                SmtpClient mySmtpClient = new SmtpClient("my.smtp.exampleserver.net");

                // set smtp-client with basicAuthentication
                mySmtpClient.UseDefaultCredentials = false;
                System.Net.NetworkCredential basicAuthenticationInfo = new
                   System.Net.NetworkCredential("username", "password");
                mySmtpClient.Credentials = basicAuthenticationInfo;

                // add from,to mailaddresses
                MailAddress from = new MailAddress("test@example.com", "TestFromName");
                MailAddress to = new MailAddress("test2@example.com", "TestToName");
                MailMessage myMail = new System.Net.Mail.MailMessage(from, to);

                // add ReplyTo
                MailAddress replyTo = new MailAddress("reply@example.com");
                myMail.ReplyToList.Add(replyTo);

                // set subject and encoding
                myMail.Subject = "Test message";
                myMail.SubjectEncoding = System.Text.Encoding.UTF8;

                // set body-message and encoding
                myMail.Body = "<b>Test Mail</b><br>using <b>HTML</b>.";
                myMail.BodyEncoding = System.Text.Encoding.UTF8;
                // text or html
                myMail.IsBodyHtml = true;

                mySmtpClient.Send(myMail);

                return true;
            }

            catch (SmtpException ex)
            {
                //throw new ApplicationException("SmtpException has occured: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }
        }

        public bool SendEmailForOrder(string userEmail, string subject, string body)
        {
            string fromMail = "devense.noreply@gmail.com";
            string fromPassword = "qgpfuvewwxgbrctz";


            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromMail);
            mailMessage.Subject = subject;
            mailMessage.To.Add(new MailAddress(userEmail));

            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential(fromMail, fromPassword),
                EnableSsl = true
            };

            try
            {
                client.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
