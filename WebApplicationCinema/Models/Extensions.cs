namespace WebApplicationCinema.Models
{
    public static class Extensions
    {
        private static List<string> _days = new List<string>() { "Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };

        private static List<string> _months = new List<string>() { "Января", "Февраля", "Марта", "Апреля", "Мая", "Июня", "Июля", "Августа", "Сентября", "Октября", "Ноября", "Декабря" };

        public static string ToWeekDay(this DateOnly date) => date.ToDateTime(new TimeOnly()) == DateTime.Today ? "Сегодня" : _days[(int)date.DayOfWeek];

        public static string ToDateMonth(this DateOnly date) => $"{date.Day} {_months[date.Month - 1]}";

        public static string Reverse(this DateOnly date)
        {
            return $"{date.Year}.{date.Month}.{date.Day}";
        }

        public static string ToSqlString(this DateOnly date)
        {
            return $"{date.Year}-{date.Month}-{date.Day}";
        }

        public static string ToSqlString(this DateOnly? date)
        {
            if (date is null)
                throw new ArgumentNullException(nameof(date));

            return date.Value.ToSqlString();
        }
    }
}
