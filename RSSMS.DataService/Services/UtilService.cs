using RSSMS.DataService.Responses;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace RSSMS.DataService.Services
{
    public static class TimeUtilStatic
    {
        public static TimeSpan StringToTime(string input)
        {
            TimeSpan result;
            var meo = input.Split(" - ");
            var time1 = meo[0];
            var tho = time1.Split(new[] { 'a', 'p' });
            int index = input.IndexOf('a');
            result = TimeSpan.FromHours(double.Parse(tho[0]));
            if (index <= 0)
            {
                if (result.Hours != 12) result += TimeSpan.FromHours(12);
            }
            return result;
        }

        public static string TimeToString(TimeSpan time)
        {
            int hour1 = time.Hours;
            int hour2 = hour1 + 2;
            string tmp1 = "am";
            string tmp2 = "am";
            if (hour1 >= 12)
            {
                if (hour1 != 12) hour1 = hour1 - 12;
                tmp1 = "pm";
            }

            if (hour2 >= 12)
            {
                if (hour2 != 12) hour2 = hour2 - 12;
                tmp2 = "pm";
            }
            return hour1 + tmp1 + " - " + hour2 + tmp2;
        }
    }
    public interface IUtilService
    {
        bool ValidateEmail(string email);
        bool ValidatePassword(string password);
        bool ValidatePhonenumber(string phonenumber);
        bool ValidateString(string input, string name);
        bool ValidateBirthDate(DateTime? date);
        bool ValidateDecimal(Decimal? number, string name);
        bool ValidateInt(int? number, string name);
        TimeSpan StringToTime(string input);
        string TimeToString(TimeSpan time);
    }
    public class UtilService : IUtilService
    {
        public UtilService()
        {
        }

        public TimeSpan StringToTime(string input)
        {
            TimeSpan result;
            var meo = input.Split(" - ");
            var time1 = meo[0];
            var tho = time1.Split(new[] { 'a', 'p' });
            int index = input.IndexOf('a');
            result = TimeSpan.FromHours(double.Parse(tho[0]));
            if (index <= 0)
            {
                if (result.Hours != 12) result += TimeSpan.FromHours(12);
            }
            return result;
        }

        public string TimeToString(TimeSpan time)
        {
            int hour1 = time.Hours;
            int hour2 = hour1 + 2;
            string tmp1 = "am";
            string tmp2 = "am";
            if (hour1 >= 12)
            {
                if (hour1 != 12) hour1 = hour1 - 12;
                tmp1 = "pm";
            }

            if (hour2 >= 12)
            {
                if (hour2 != 12) hour2 = hour2 - 12;
                tmp2 = "pm";
            }
            return hour1 + tmp1 + " - " + hour2 + tmp2;
        }

        public bool ValidateBirthDate(DateTime? date)
        {
            if (date == null) return true;
            TimeSpan span = DateTime.Now - (DateTime)date;
            DateTime zeroTime = new DateTime(1, 1, 1);
            int years = (zeroTime + span).Year - 1;
            if (years <= 17 || years >= 99) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập ngày sinh đúng");
            return true;
        }

        public bool ValidateDecimal(decimal? number, string name)
        {
            if (number == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập " + name);
            if (number < 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập " + name + " đúng");
            return true;
        }

        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Tài khoản / mật khẩu không đúng");
            string[] list = new[] { "~", "`", "!", "#", "$", "%", "^", "&", "*", "(", ")", "+", "=", "\"" };
            if (list.Any(email.Contains)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email contains special character");
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
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập đúng email");
            }
            catch (ArgumentException)
            {
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập đúng email");
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập đúng email");
            }
        }

        public bool ValidateInt(int? number, string name)
        {
            if (number == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập " + name);
            return true;
        }

        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Tài khoản / mật khẩu không đúng");
            if (password.Length <= 5) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập mật khẩu hơn 6 ký tự");
            return true;
        }

        public bool ValidatePhonenumber(string phonenumber)
        {
            if (string.IsNullOrWhiteSpace(phonenumber)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập số điện thoại");
            if (phonenumber.Length != 10) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập số điện thoại đúng");

            var regexItem = new Regex(@"^((0(\d){9}))*$");
            if (!regexItem.IsMatch(phonenumber)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Số điện thoại sai định dạng");
            return true;
        }

        public bool ValidateString(string input, string name)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập " + name);

            return true;
        }

    }
}
