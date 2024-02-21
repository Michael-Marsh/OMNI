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
