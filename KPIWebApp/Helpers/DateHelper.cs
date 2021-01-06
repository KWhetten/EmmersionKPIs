using System;

namespace KPIWebApp.Helpers
{
    public class DateHelper
    {
        public static DateTimeOffset GetStartDate(string startDateString)
        {
            if (startDateString == null)
            {
                return new DateTimeOffset(new DateTime(2020, 10, 19));
            }

            try
            {
                var startDate = new DateTimeOffset(Convert.ToDateTime(startDateString).Date);

                return startDate == new DateTimeOffset(DateTime.MinValue)
                    ? new DateTimeOffset(DateTime.Now)
                    : startDate;
            }
            catch (Exception ex)
            {
                return new DateTimeOffset(new DateTime(2020, 10, 19));
            }
        }

        public static DateTimeOffset GetStartDate(DateTimeOffset startDate)
        {
            return startDate.Date;
        }

        public static DateTimeOffset? GetStartDate(DateTimeOffset? startDate)
        {
            return startDate?.Date;
        }

        public static DateTimeOffset GetFinishDate(string endDateString)
        {
            if (endDateString == null)
            {
                return new DateTimeOffset(DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59));
            }
            try
            {
                var finishDate = new DateTimeOffset(Convert.ToDateTime(endDateString).Date.AddHours(23).AddMinutes(59).AddSeconds(59));
                return finishDate;
            }
            catch (Exception ex)
            {
                return new DateTimeOffset(DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59));
            }
        }

        public static DateTimeOffset GetFinishDate(DateTimeOffset finishDate)
        {
            return finishDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        public static DateTimeOffset? GetFinishDate(DateTimeOffset? finishDate)
        {
            return finishDate?.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }
    }
}
