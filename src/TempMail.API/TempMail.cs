using CloudFlareUtilities;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using TempMail.Constants;
using TempMail.Extensions;
using TempMail.Models;
using TempMail.Utilities;

namespace TempMail
{
    public class TempMailClient
    {
        private CookieContainer CookieContainer;

        private List<string> _availableDomains;
        public List<string> AvailableDomains => _availableDomains ?? (_availableDomains = GetAvailableDomains());

        public string User { get; set; }
        public string Domain { get; set; }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { _email = value; OnEmailChanged(); }
        }

        protected List<Mail> Mails = new List<Mail>();

        public HttpClient client { get; private set; }

        public TempMailClient() { TempMailInitializer(null, null); }
        public TempMailClient(string login) { TempMailInitializer(login, null); }
        public TempMailClient(string login, string domain) { TempMailInitializer(login, domain); }

        private void TempMailInitializer(string login, string domain)
        {
            CookieContainer = new CookieContainer();

            if (!CreateSession())
                return;
            if (domain != null && !AvailableDomains.Contains(Functions.NormalizeDomain(domain)))
                throw new Exception(string.Format("The domain you entered is not an available domain: {0}", domain));
            if (IsInvalidLogin(login, domain))
                return;
            if (domain == null)
                domain = AvailableDomains[new Random().Next(0, AvailableDomains.Count)];

            ChangeEmail(login, domain);
        }

        /// <summary>
        /// Starts a new client session and get a new temporary email
        /// </summary>
        private bool CreateSession()
        {
            this.client = CreateHttpClient();

            var document = GetHtmlDocument(ConstantsUrL.URL_BASE);
            this.Email = Functions.GetMailAddress(document);

            return Email != null;
        }
        
        /// <summary>
        /// Trigger for set up User and Domain from Email
        /// </summary>
        private void OnEmailChanged()
        {
            if (!string.IsNullOrEmpty(Email))
            {
                this.User = Email.Substring(0, Email.IndexOf('@'));
                this.Domain = Email.Substring(Email.IndexOf('@') + 1);
            }
        }

        /// <summary>
        /// Changes the temporary email. E.g. login@domain
        /// </summary>
        /// <param name="login">New temporary email login</param>
        /// <param name="domain">New temporary email domain</param>
        public string ChangeEmail(string login, string domain)
        {
            client.DefaultRequestHeaders.Add("Referer", ConstantsUrL.URL_CHANGE);
            var data = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("csrf",  CookieContainer.GetSpecificCookie("csrf").Value),
                new KeyValuePair<string, string>("mail", login),
                new KeyValuePair<string, string>("domain", Functions.NormalizeDomain(domain)),
             });

            var response = client.PostAsync(ConstantsUrL.URL_CHANGE, data).GetAwaiter().GetResult();
            client.DefaultRequestHeaders.Remove("Referer");

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            return (Email = $"{login}{Functions.NormalizeDomain(domain)}");
        }

        /// <summary>
        /// Deletes the temporary email and gets a new one.
        /// </summary>
        public bool Delete()
        {            
            var response = client.GetRequest(ConstantsUrL.URL_DELETE);
            if (response.StatusCode != HttpStatusCode.OK)
                return false;

            Email = (JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content))?["mail"].ToString();
            CookieContainer.UpdateEmailCookie(Email);

            return true;
        }
               
        /// <summary>
        /// Checks if the input class parameters are valid
        /// </summary>
        private bool IsInvalidLogin(string login, string domain)
        {
            return string.IsNullOrEmpty(login) || string.IsNullOrEmpty(domain);
        }



        /// <summary>
        /// Return all mails into the inbox
        /// </summary>
        public IEnumerable<Mail> GetMails()
        {
            var document = GetHtmlDocument(ConstantsUrL.URL_CHECK);
            Mails.AddRange(GetNewMails(ParseMails(document)));

            return Mails;
        }

        private IEnumerable<Mail> ParseMails(HtmlDocument document)
        {
            var rawMails = document.DocumentNode.SelectSingleNode("//div[@class='inbox-dataList']")?.Descendants("li");
            return rawMails != null ? rawMails.Select(ParseSingleMail).ToList() : new List<Mail>();
        }

        private Mail ParseSingleMail(HtmlNode node)
        {
            var a = node.SelectSingleNode("//div[@class='m-link-view']/a");
            var link = a.GetAttributeValue("href", null);

            return new Mail()
            {
                Id = Functions.GetMailIDFromLink(link),
                Subject = node.GetElementsByClassName("title-subject").FirstOrDefault()?.InnerText,
                StrSender = a?.InnerText,
                Link = link
            };
        }

        /// <summary>
        /// Returns the mails box
        /// </summary>
        private IEnumerable<Mail> GetNewMails(IEnumerable<Mail> mails)
        {
            return mails.Where(mail => Mails.Count(m => m.Id == mail.Id) == 0).Select(mail => Mail.FromId(this, mail.Id)).ToList();
        }


        /// <summary>
        /// Returns all available domains
        /// </summary>
        private List<string> GetAvailableDomains()
        {
            var document = GetHtmlDocument(ConstantsUrL.URL_CHANGE);
            return document.GetElementbyId("domain").Descendants("option").Select(s => s.GetAttributeValue("value", null)).ToList();
        }

        /// <summary>
        /// Sends a get request to the Url provided using this session cookies and returns the HtmlDocument of the result.
        /// </summary>
        private HtmlDocument GetHtmlDocument(string url)
        {
            var response = client.GetRequest(url);
            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var document = new HtmlDocument();
            document.LoadHtml(client.GetRequest(url).Content);
            return document;
        }

        /// <summary>
        /// Returns an web client that have this session cookies.
        /// </summary>
        private HttpClient CreateHttpClient()
        {
            // Handler for bypass Cloudflare
            var handler = new ClearanceHandler()
            {
                InnerHandler = new HttpClientHandler() { CookieContainer = CookieContainer },
                MaxRetries = 10,
                ClearanceDelay = 3000
            };

            var client = new HttpClient(handler);
            //var client = new HttpClient(new HttpClientHandler() { CookieContainer = CookieContainer });

            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.109 Safari/537.36");
            client.DefaultRequestHeaders.Add("Host", ConstantsUrL.URL_DOMAIN);
            client.DefaultRequestHeaders.Add("Origin", "https://" + ConstantsUrL.URL_DOMAIN);
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");

            return client;
        }
        
    }

  
}
