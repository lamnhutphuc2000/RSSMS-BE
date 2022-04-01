using RSSMS.DataService.Responses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace RSSMS.DataService.Services
{
    public interface IUtilService 
    {
        bool ValidateEmail(string email);
        bool ValidatePassword(string password);
        bool ValidatePhonenumber(string phonenumber);
        bool ValidateString(string input, string name);
        bool ValidateBirthDate(DateTime? date);
    }
    public class UtilService : IUtilService
    {
        public UtilService()
        {
        }

        public bool ValidateBirthDate(DateTime? date)
        {
            if (date == null) return true;
            TimeSpan span = DateTime.Now - (DateTime)date;
            DateTime zeroTime = new DateTime(1, 1, 1);
            int years = (zeroTime + span).Year - 1;
            if (years <= 12 || years >= 99) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Invalid birthdate");
            return true;
        }

        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email can not null or have whitespace");
            string[] list = new[] { "~", "`", "!", "#", "$", "%", "^", "&", "*", "(", ")", "+", "=", "\"" };
            if(list.Any(email.Contains)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email contains special character");
            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    IdnMapping idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email invalid format");
            }
            catch (ArgumentException)
            {
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email invalid format");
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email invalid format");
            }
        }

        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Password can not null or have whitespace");
            if(password.Length <= 5) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Password must have atleast 6 characters");
            return true;
        }

        public bool ValidatePhonenumber(string phonenumber)
        {
            if (string.IsNullOrWhiteSpace(phonenumber)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Phone number can not null or have whitespace");
            if(phonenumber.Length != 10) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Phone number must have 10 characters");
            var regexItem = new Regex("^[0-9 ]*$");

            if (!regexItem.IsMatch(phonenumber)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Phone number invalid format");
            return true;
        }

        public bool ValidateString(string input, string name)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, name + " can not null or have whitespace");

            return true;
        }
    }
}
