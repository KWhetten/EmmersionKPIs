using System;

namespace KPIWebApp.Helpers
{
    public class DateHelper
    {
        public static DateTimeOffset GetStartDate(string startDateString)
        {
            if (startDateString == null)
            {
                return new DateTimeOffset(new DateTime(2015, 1, 1));
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
                return new DateTimeOffset(new DateTime(2015, 1, 1));
            }
        }

        public static DateTimeOffset GetFinishDate(string endDateString)
        {
            if (endDateString == null)
            {
                return new DateTimeOffset(DateTime.Now.AddDays(1).Date);
            }

            try
            {
                var finishDate = new DateTimeOffset(Convert.ToDateTime(endDateString).Date);

                return finishDate;
            }
            catch (Exception ex)
            {
                return new DateTimeOffset(new DateTime(2015, 1, 1));
            }
        }
    }
}
