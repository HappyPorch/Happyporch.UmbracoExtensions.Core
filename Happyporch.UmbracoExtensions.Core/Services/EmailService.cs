using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace HappyPorch.UmbracoExtensions.Core.Services
{
    public static class EmailService
    {
        /// <summary>
        /// Send an email using default SMTP settings
        /// </summary>
        /// <param name="fromEmailAddress"></param>
        /// <param name="toEmailAddresses"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="ccEmailAddresses"></param>
        /// <param name="bccEmailAddresses"></param>
        /// <param name="isBodyHtml"></param>
        /// <param name="files"> Use Request.Files["filename"] to get file. Form needs to have enctype="multipart/form-data".</param>
        /// <param name="fromName">Display name</param>
        /// <param name="replyTo"></param>
        /// <param name="replyToName">Display name</param>
        public static void SendEmail(string fromEmailAddress, IEnumerable<string> toEmailAddresses, string subject, string body, IEnumerable<string> ccEmailAddresses = null, IEnumerable<string> bccEmailAddresses = null, bool isBodyHtml = false, HttpFileCollection files = null, string fromName = "", string replyTo = "", string replyToName = "")
        {
            var filesWrapper = new HttpFileCollectionWrapper(files);

            SendEmail(fromEmailAddress, toEmailAddresses, subject, body, ccEmailAddresses, bccEmailAddresses, isBodyHtml, filesWrapper, fromName, replyTo, replyToName);
        }

        /// <summary>
        /// Send an email using default SMTP settings
        /// </summary>
        /// <param name="fromEmailAddress"></param>
        /// <param name="toEmailAddresses"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="ccEmailAddresses"></param>
        /// <param name="bccEmailAddresses"></param>
        /// <param name="isBodyHtml"></param>
        /// <param name="files"> Use Request.Files["filename"] to get file. Form needs to have enctype="multipart/form-data".</param>
        /// <param name="fromName">Display name</param>
        /// <param name="replyTo"></param>
        /// <param name="replyToName">Display name</param>
        public static void SendEmail(string fromEmailAddress, IEnumerable<string> toEmailAddresses, string subject, string body, IEnumerable<string> ccEmailAddresses = null, IEnumerable<string> bccEmailAddresses = null, bool isBodyHtml = false, HttpFileCollectionBase files = null, string fromName = "", string replyTo = "", string replyToName = "")
        {
            // Skips sending email. For testing on ghost inspector. 
            if (body != null && body.Contains("[skip email]")) { return; }
            using (var smtpClient = new SmtpClient())
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmailAddress, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml,
                    BodyEncoding = Encoding.UTF8,
                    Priority = MailPriority.Normal,
                };

                foreach (var recipient in toEmailAddresses)
                {
                    mail.To.Add(recipient);
                }
                foreach (var recipient in ccEmailAddresses)
                {
                    mail.CC.Add(recipient);
                }
                foreach (var recipient in bccEmailAddresses)
                {
                    mail.Bcc.Add(recipient);
                }
                if (!string.IsNullOrEmpty(replyTo))
                {
                    mail.ReplyToList.Add(new MailAddress(replyTo, !string.IsNullOrEmpty(replyToName) ? "On behalf of " + replyToName : ""));
                }

                if (files != null && files.Count > 0)
                {
                    foreach (string key in files)
                    {
                        HttpPostedFileBase file = files[key];
                        if (file == null || file.ContentLength == 0) continue;
                        string fileName = Path.GetFileName(file.FileName);
                        var attachment = new Attachment(file.InputStream, fileName);
                        mail.Attachments.Add(attachment);
                    }
                }
                smtpClient.Send(mail);
            }
        }
    }

}

