using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using DataAccess.Objects;
using Microsoft.Extensions.Configuration;

namespace KPIWebApp.Helpers
{
    public class EmailHelper
    {
        public void SendRegistrationEmail(UserInfo userInfo, string baseUrl)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            var config = builder.Build();
            var smtpClient = new SmtpClient(config["Smtp:Host"])
            {
                Port = int.Parse(config["Smtp:Port"]),
                Credentials = new NetworkCredential(File.ReadLines($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/EmmersionKPI/emailCredentials.txt").First(), File.ReadLines($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/EmmersionKPI/emailCredentials.txt").Last()),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("emmersion.kpi@gmail.com"),
                Subject = "Welcome to Emmersion KPI Monitoring",
                Body = $"<p>Hello {userInfo.FirstName} {userInfo.LastName},</p>\n" +
                       $"<p>Someone at Emmersion registered your email so you can check out the Emmersion KPI Monitoring service. Click the link below to get started!</p>\n" +
                       $"<a href=\"{baseUrl}create-password?email={userInfo.Email}\">Click here to Register!</a>\n" +
                       $"<p>Sincerely,</p>" +
                       $"<p>The Emmersion Team",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(userInfo.Email);

            smtpClient.Send(mailMessage);
        }

        public virtual bool SendForgotPasswordEmail(UserInfo userInfo, string baseUrl)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            var config = builder.Build();
            var smtpClient = new SmtpClient(config["Smtp:Host"])
            {
                Port = int.Parse(config["Smtp:Port"]),
                Credentials = new NetworkCredential(File.ReadLines($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/EmmersionKPI/emailCredentials.txt").First(), File.ReadLines($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/EmmersionKPI/emailCredentials.txt").Last()),
                EnableSsl = true,
            };
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("emmersion.kpi@gmail.com"),
                    Subject = "Welcome to Emmersion KPI Monitoring",
                    Body = $"<p>Hello {userInfo.FirstName} {userInfo.LastName},</p>\n" +
                           "<p>Someone at Emmersion registered your email so you can check out the Emmersion KPI Monitoring service. Click the link below to get started!</p>\n" +
                           $"<a href=\"{baseUrl}create-password?email={userInfo.Email}\">Click here to Register!</a>\n" +
                           "<p>Sincerely,</p>" +
                           "<p>The Emmersion Team",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(userInfo.Email);

                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
