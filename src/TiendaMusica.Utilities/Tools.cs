namespace TiendaMusica.Utilities
{
    public class Tools : ITools
    {
        private readonly string _dateFormat = "dd-MM-yyyy HH:mm:ss";

        public DateTime DateTimeUtcToBogota(DateTime dateTimeUtc)
        {
            DateTime universalDate = dateTimeUtc.Kind switch
            {
                DateTimeKind.Local => dateTimeUtc.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc),
                _ => dateTimeUtc
            };

            TimeZoneInfo bogotaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(universalDate, bogotaTimeZone);
        }

        public string DateTimeUtcToBogotaAsString(DateTime dateTimeUtc)
        {
            return DateTimeUtcToBogota(dateTimeUtc).ToString(_dateFormat);
        }
    }
}
