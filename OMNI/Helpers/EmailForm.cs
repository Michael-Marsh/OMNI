using System;
using System.Net;
using System.Net.Mail;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OMNI.Helpers
{
    /// <summary>
    /// Email OMNI forms Interaction Logic
    /// </summary>
    public class EmailForm
    {
        /// <summary>
        /// OMNI Email Form Constructor
        /// </summary>
        public EmailForm()
        {

        }

        /// <summary>
        /// e-Mail an OMNI form with attachment(s).
        /// </summary>
        /// <param name="receiver">e-Mail recipient</param>
        /// <param name="messageBody">e-Mail message</param>
        /// <param name="messageSubject">e-Mail subject</param>
        /// <param name="attachment">Any e-Mail Attachments</param>
        public static void SendwithAttachment(string receiver, string messageBody, string messageSubject, params string[] attachment)
        {
            try
            {
                var credentials = new NetworkCredential(Properties.Settings.Default.omniEmailAddress, Properties.Settings.Default.EmailPassword, Properties.Settings.Default.EmailDomain);
                using (var client = new SmtpClient { Port = 587, Host = Properties.Settings.Default.SmtpDefaultClient, DeliveryMethod = SmtpDeliveryMethod.Network, UseDefaultCredentials = false, Credentials = credentials, EnableSsl = true })
                {
                    messageBody = $"{messageBody}\n\n Please do not reply to this email. Any responses to this email will be lost.";
                    using (var email = new MailMessage { From = new MailAddress(Properties.Settings.Default.omniEmailAddress), Subject = messageSubject, Body = messageBody, IsBodyHtml = false })
                    {
                        email.To.Add(new MailAddress(receiver));
                        foreach (string file in attachment)
                        {
                            using (var attachObject = new Attachment(file))
                            {
                                email.Attachments.Add(attachObject);
                                client.Send(email);
                            }
                        }
                    }
                }
            }
            catch (Exception) { return; }
        }

        /// <summary>
        /// e-Mail from OMNI with out attachments.
        /// </summary>
        /// <param name="receiver">e-Mail recipient</param>
        /// <param name="messageBody">e-Mail message</param>
        /// <param name="messageSubject">e-Mail subject</param>
        public static void SendwithoutAttachment(string receiver, string messageBody, string messageSubject)
        {
            try
            {
                var credentials = new NetworkCredential(Properties.Settings.Default.omniEmailAddress, Properties.Settings.Default.EmailPassword, Properties.Settings.Default.EmailDomain);
                using (var client = new SmtpClient { Port = 587, Host = Properties.Settings.Default.SmtpDefaultClient, DeliveryMethod = SmtpDeliveryMethod.Network, UseDefaultCredentials = false, Credentials = credentials, EnableSsl = true })
                {
                    messageBody = $"{messageBody}\n\nPlease do not reply to this email. Any responses to this email will be lost.";
                    using (var email = new MailMessage { From = new MailAddress(Properties.Settings.Default.omniEmailAddress), Subject = messageSubject, Body = messageBody, IsBodyHtml = false })
                    {
                        email.To.Add(new MailAddress(receiver));
                        client.Send(email);
                    }
                }
            }
            catch (Exception) { return; }
        }

        /// <summary>
        /// Manually e-Mail from OMNI with Microsoft Outlook
        /// </summary>
        /// <param name="messageSubject">e-Mail Subject</param>
        /// <param name="attachment">e-Mail Attachments</param>
        public static void ManualSend(string messageSubject, params string[] attachment)
        {
            try
            {
                var outlookApp = new Outlook.Application();
                var mailItem = (Outlook._MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);
                mailItem.Subject = messageSubject;
                var i = 1;
                if (attachment == null || attachment.Length != 0)
                {
                    foreach (string file in attachment)
                    {
                        mailItem.Attachments.Add(file, Outlook.OlAttachmentType.olByValue, i);
                        i++;
                    }
                }
                mailItem.Display(true);
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
        }

        /// <summary>
        /// Manually e-Mail from OMNI with Microsoft Outlook
        /// </summary>
        /// <param name="reciever">e-Mail recipient</param>
        /// <param name="messageSubject">e-Mail Subject</param>
        /// <param name="async">todo: build for an async call</param>
        public static void ManualSend(string reciever, string messageSubject, bool async)
        {
            try
            {
                async = false;
                var outlookApp = new Outlook.Application();
                var mailItem = (Outlook._MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);
                mailItem.Subject = messageSubject;
                mailItem.To = reciever;
                mailItem.Display(true);
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
        }
    }
}
