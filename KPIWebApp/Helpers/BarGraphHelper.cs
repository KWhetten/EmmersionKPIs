using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.Statistics.Kernels;
using DataAccess.DataRepositories;
using DataAccess.Objects;

namespace KPIWebApp.Helpers
{
    public class BarGraphHelper
    {
        public async Task<BarGraphData> GetReleaseBarGraphData(DateTimeOffset startDate, DateTimeOffset finishDate,
            bool assessmentsTeam, bool enterpriseTeam)
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
            var removeList = new List<Release>();

            foreach (var release in releases)
            {
                if (release.ReleaseEnvironment.Name.Contains("Assessments") && !assessmentsTeam)
                {
                    removeList.Add(release);
                }

                if ((release.ReleaseEnvironment.Name.Contains("TrueNorthTest Release")
                    || release.ReleaseEnvironment.Name.Contains("Production"))
                    && !enterpriseTeam)
                {
                    removeList.Add(release);
                }
            }

            foreach (var release in removeList)
            {
                releases.Remove(release);
            }

            var rawReleaseData = new Dictionary<DateTimeOffset, int>();
            var rawRolledBackData = new Dictionary<DateTimeOffset, int>();
            var currentDate = startDate.Date;
            var dateStrings = new List<string>();

            while (currentDate <= finishDate)
            {
                rawReleaseData.Add(currentDate.Date, 0);
                rawRolledBackData.Add(currentDate.Date, 0);
                dateStrings.Add(currentDate.ToString("MMMM dd"));
                currentDate = currentDate.AddDays(1);
            }

            var releaseHelper = new ReleaseHelper();
            var rolledBackReleases = releaseHelper.GetRolledBackReleases(releases);

            foreach (var release in rolledBackReleases)
            {
                releases.Remove(release);
                rawRolledBackData[release.FinishTime.Value.Date]++;
            }

            foreach (var release in releases
                .Where(release => release.FinishTime.Value.Date >= startDate
                                  && release.FinishTime.Value.Date < finishDate))
            {
                rawReleaseData[release.FinishTime.Value.Date]++;
            }

            data.Rows[0].Name = "Releases";
            data.Rows[0].Data = rawReleaseData.Values.ToList();

            data.Rows[1].Name = "Rolled Back Releases";
            data.Rows[1].Data = rawRolledBackData.Values.ToList();

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
