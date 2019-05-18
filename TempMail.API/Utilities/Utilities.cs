using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TempMail.Constants;

namespace TempMail.Utilities
{
    public static class Functions
    {
        /// <summary>
        /// Returns the normalized domain string
        /// </summary>
        public static string NormalizeDomain(string domain)
        {
            return (domain[0] == '@') ? domain : '@' + domain;
        }

        /// <summary>
        /// Return the specific cookie value from temp-mail.org
        /// </summary>
        public static Cookie GetSpecificCookie(this CookieContainer CookieContainer, string Name)
        {
            return CookieContainer.GetCookies(new Uri(ConstantsUrL.URL_BASE))[Name];
        }

        /// <summary>
        /// Update the mail address into cookie container
        /// </summary>
        public static void UpdateEmailCookie(this CookieContainer CookieContainer, string Email)
        {
            CookieContainer.SetCookies(new Uri(ConstantsUrL.URL_BASE), $"mail={Email}");
        }

        /// <summary>
        /// Returns the email address
        /// </summary>
        public static string GetMailAddress(HtmlDocument document)
        {
            return document.GetElementbyId("mail").GetAttributeValue("value", null);
        }

        /// <summary>
        /// Returns the email if from link
        /// </summary>
        public static string GetMailIDFromLink(string link)
        {
            return Regex.Match(link, ConstantsUrL.URL_BASE + "/" + @".*?/(?<id>.*)").Groups["id"].Value;
        }

    }
}
