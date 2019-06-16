using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TempMail.Constants;
using TempMail.Extensions;
using TempMail.Utilities;

namespace TempMail.Models
{
    public class Mail
    {
        public string Subject { get; set; }
        public string From { get; private set; }
        public string To { get; private set; }

        public IEnumerable<MimePart> Attachments { get; private set; }
        public string Content { get; private set; }
        public string ContentHtml { get; private set; }
        public DateTimeOffset? Date { get; set; }

        public string Id { get; set; }
        public string Link { get; set; }
        public string StrSender { get; set; }

        public Mail() { }
        
        public static Mail FromId(TempMailClient session, string id)
        {
            var sourceUrl = $"{ConstantsUrL.URL_SOURCE}{id}";
            var mailRaw = session.client.GetString(sourceUrl);

            return GetMailFromRaw(mailRaw, id);
        }
        public static Mail FromLink(TempMailClient session, string link)
        {
            var id = Functions.GetMailIDFromLink(link);
            var sourceUrl = $"{ConstantsUrL.URL_SOURCE}{id}";
            var mailRaw = session.client.GetString(sourceUrl);

            return GetMailFromRaw(mailRaw, id);
        }

        private static Mail GetMailFromRaw(string rawMail, string id)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(rawMail ?? ""));
            var mail = ConvertMimeMessageToMail(MimeMessage.Load(stream));

            mail.Id = id;
            return mail;
        }

        private static Mail ConvertMimeMessageToMail(MimeMessage message)
        {
            return new Mail
            {
                Subject = message.Subject,

                From = message.From.ToString(),
                To = message.To.ToString(),

                Attachments = message.Attachments.Cast<MimePart>(),
                Content = System.Net.WebUtility.HtmlDecode(Uri.UnescapeDataString(message.TextBody ?? string.Empty)),
                ContentHtml = System.Net.WebUtility.HtmlDecode(Uri.UnescapeDataString(message.HtmlBody ?? string.Empty)),
                Date = message.Date,
            };

        }

        public void SaveAttachment(MimePart attachment, string directory = "", string altFileName = null)
        {
            var fileName = attachment.FileName ?? altFileName ?? $"file{ new Random().Next(10000) }";
            using (var stream = File.Create(Path.Combine(directory, fileName)))
                attachment.Content.DecodeTo(stream);
        }

    }
}
