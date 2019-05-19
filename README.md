# TempMail-API
Unofficial API for [TempMail](https://temp-mail.org) in .NET Standard

# Usage
```csharp
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
```
# Building Code
[.NET Standard 2.0](https://github.com/dotnet/standard/blob/master/docs/versions.md)
[.NET Framework 4.6]

# Dependencies
* [CloudFlare Utilities](https://github.com/elcattivo/CloudFlareUtilities)
* [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack)
* [MimeKit](https://www.nuget.org/packages/MimeKit)
* [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json)
