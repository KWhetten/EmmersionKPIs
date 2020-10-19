using System;

namespace KPIWebApp.Helpers
{
    public class DateHelper
    {
        public static DateTime GetStartDate(string startDateString)
        {
            if (startDateString == null)
            {
                return new DateTime(2015, 1, 1);
            }

            try
            {
                var startDate = Convert.ToDateTime(startDateString).Date;

                return startDate == DateTime.MinValue
                    ? DateTime.Today
                    : startDate;
            }
            catch (Exception ex)
            {
                return new DateTime(2015, 1, 1);
            }
        }

        public static DateTime GetFinishDate(string endDateString)
        {
            if (endDateString == null)
            {
                return DateTime.Today.AddDays(1);
            }

            try
            {
                var startDate = Convert.ToDateTime(endDateString).Date;

                return startDate == DateTime.MinValue
                    ? DateTime.Today
                    : startDate;
            }
            catch (Exception ex)
            {
                return new DateTime(2015, 1, 1);
            }
        }
    }
}
