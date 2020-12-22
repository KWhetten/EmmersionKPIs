using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.Statistics.Kernels;
using DataAccess.DataRepositories;

namespace KPIWebApp.Helpers
{
    public class BarGraphHelper
    {
        public async Task<BarGraphData> GetReleaseBarGraphData(DateTimeOffset startDate, DateTimeOffset finishDate)
        {
            var data = new BarGraphData
            {
                Rows = new List<BarGraphDataRow>
                {
                    new BarGraphDataRow(),
                    new BarGraphDataRow()
                }
            };
            var releaseRepository = new ReleaseRepository();

            var releases = (await releaseRepository.GetReleaseListAsync(startDate, finishDate)).ToList();

            DateTimeOffset? earliestDate = finishDate;
            DateTimeOffset? latestDate = startDate;

            foreach (var release in releases)
            {
                if (release.FinishTime != null && release.FinishTime.Value.Date != startDate
                                               && release.FinishTime.Value.Date < earliestDate)
                {
                    earliestDate = release.FinishTime.Value.Date;
                }

                if (release.FinishTime != null && release.FinishTime.Value.Date != finishDate
                                               && release.FinishTime.Value.Date > latestDate)
                {
                    latestDate = release.FinishTime.Value.Date;
                }
            }

            var rawReleaseData = new Dictionary<DateTimeOffset, int>();
            var rawRolledBackData = new Dictionary<DateTimeOffset, int>();
            var currentDate = earliestDate.Value.Date;
            var dateStrings = new List<string>();

            while (currentDate <= latestDate)
            {
                rawReleaseData.Add(currentDate.Date, 0);
                rawRolledBackData.Add(currentDate.Date, 0);
                currentDate = currentDate.AddDays(1);
                dateStrings.Add(currentDate.ToString("MMMM dd"));
            }

            var releaseHelper = new ReleaseHelper();
            var rolledBackReleases = releaseHelper.GetRolledBackReleases(releases);

            foreach (var release in rolledBackReleases)
            {
                releases.Remove(release);
                rawRolledBackData[release.FinishTime.Value.Date]++;
            }

            foreach (var release in releases.Where(release => release.FinishTime.Value.Date > earliestDate && release.FinishTime.Value.Date < latestDate))
            {
                rawReleaseData[release.FinishTime.Value.Date]++;
            }

            data.Rows[0].Name = "Rolled Back Releases";
            data.Rows[0].Data = rawRolledBackData.Values.ToList();

            data.Rows[1].Name = "Releases";
            data.Rows[1].Data = rawReleaseData.Values.ToList();

            data.Dates = dateStrings;

            return data;
        }
    }

    public class BarGraphData
    {
        public List<BarGraphDataRow> Rows { get; set; }
        public List<string> Dates { get; set; }
    }

    public class BarGraphDataRow
    {
        public string Name { get; set; }
        public List<int> Data { get; set; }
    }
}
