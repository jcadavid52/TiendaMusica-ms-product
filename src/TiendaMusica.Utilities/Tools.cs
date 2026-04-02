using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace TiendaMusica.Utilities
{
    public class Tools : ITools
    {
        private readonly string _dateFormat = "dd-MM-yyyy HH:mm:ss";

        public string SHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {

                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public string GetEntropyIdTail(string id)
        {
            string hash = SHA256Hash(id);
            return hash is not null ? hash.Substring(0, 6) : null;
        }

        public long ToUnixEpoch(DateTime dateTime)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
            return dateTimeOffset.ToUnixTimeMilliseconds();
        }

        public DateTime ToDateTime(long unixEpoch)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixEpoch);
            return dateTimeOffset.DateTime;
        }

        public DateTime DateTimeUtcToBogota(DateTime dateTimeUtc)
        {
            TimeZoneInfo bogotaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, bogotaTimeZone);
        }

        public string DateTimeUtcToBogotaAsString(DateTime dateTimeUtc)
        {
            return DateTimeUtcToBogota(dateTimeUtc).ToString(_dateFormat);
        }

        public DateTime DateTimeBogotaToUtc(DateTime dateTimeBogota)
        {
            DateTime bogotaTimeWithKind = DateTime.SpecifyKind(dateTimeBogota, DateTimeKind.Unspecified);
            TimeZoneInfo bogotaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

            return TimeZoneInfo.ConvertTimeToUtc(bogotaTimeWithKind, bogotaTimeZone);
        }

        public string DateTimeBogotaToUtcAsString(DateTime dateTime)
        {
            return DateTimeBogotaToUtc(dateTime).ToString(_dateFormat);
        }

        public DateTime DateTimeBogotaToUtc(string dateTimeBogota)
        {
            try
            {
                DateTime bogota = DateTime.ParseExact(dateTimeBogota, _dateFormat, CultureInfo.InvariantCulture);
                return DateTimeBogotaToUtc(bogota);
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        public string DateTimeBogotaToUtcAsString(string dateTimeBogota)
        {
            if (dateTimeBogota is null)
            {
                return null;
            }

            return DateTimeBogotaToUtc(dateTimeBogota).ToString(_dateFormat);
        }

        public string DateTimeToString(DateTime dateTime)
        {
            return dateTime.ToString(_dateFormat);
        }

        public DateTime DateStringToDateTime(string dateTimeString)
        {
            try
            {
                return DateTime.ParseExact(dateTimeString, _dateFormat, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }
    }
}
