using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Objects;
using KPIWebApp.Models;

namespace KPIWebApp.Helpers
{
    public class ReleaseHelper
    {
        public ReleaseOverviewData PopulateOverviewData(ReleaseOverviewData releaseOverviewData,
            List<Release> releaseList, DateTimeOffset finishDate, bool assessmentsTeam, bool enterpriseTeam)
        {
            var lastReleaseByEnvironment = new Dictionary<int, Release>();
            DateTimeOffset? earliestReleaseFinishTime = null;
            var sumTimeToRestore = 0.0m;

            foreach (var item in releaseList)
            {
                if (!lastReleaseByEnvironment.ContainsKey(item.ReleaseEnvironment.Id))
                {
                    lastReleaseByEnvironment.Add(item.ReleaseEnvironment.Id, new Release());
                }

                if (item.FinishTime < earliestReleaseFinishTime || earliestReleaseFinishTime == null)
                {
                    earliestReleaseFinishTime = item.FinishTime;
                }

                if (item.Attempts > 1 && lastReleaseByEnvironment[item.ReleaseEnvironment.Id].Name != item.Name &&
                    ReleaseVersionIsLater(item.Name, lastReleaseByEnvironment[item.ReleaseEnvironment.Id].Name))
                {
                    sumTimeToRestore =
                        (decimal) (item.FinishTime -
                                   lastReleaseByEnvironment[item.ReleaseEnvironment.Id].FinishTime).Value
                        .TotalMinutes;
                }

                lastReleaseByEnvironment[item.ReleaseEnvironment.Id] = item;
            }

            var releaseWeeks = (finishDate - earliestReleaseFinishTime)?.Days / 7m;

            var rolledBackReleases = GetRolledBackReleases(releaseList);

            releaseOverviewData.TotalDeploys = releaseList.Count;
            releaseOverviewData.SuccessfulDeploys = releaseList.Count - rolledBackReleases.Count;
            releaseOverviewData.RolledBackDeploys = rolledBackReleases.Count;
            releaseOverviewData.DeployFrequency = releaseWeeks != 0
                ? decimal.Round(decimal.Parse((releaseOverviewData.TotalDeploys / releaseWeeks)?.ToString("0.##")!), 2,
                    MidpointRounding.AwayFromZero)
                : 0;
            releaseOverviewData.MeanTimeToRestore = rolledBackReleases.Count != 0
                ? decimal.Round(decimal.Parse((sumTimeToRestore / rolledBackReleases.Count).ToString("0.##")!), 2,
                    MidpointRounding.AwayFromZero)
                : 0;
            releaseOverviewData.ChangeFailPercentage = releaseList.Count != 0
                ? decimal.Round(
                    decimal.Parse(((decimal) rolledBackReleases.Count / releaseList.Count * 100).ToString("0.##")!), 2,
                    MidpointRounding.AwayFromZero)
                : 0;

            return releaseOverviewData;
        }

        public List<Release> GetRolledBackReleases(List<Release> releases)
        {
            var lastReleaseByEnvironment = new Dictionary<int, Release>();
            var rolledBackReleases = new List<Release>();
            foreach (var item in releases)
            {
                if (!lastReleaseByEnvironment.ContainsKey(item.ReleaseEnvironment.Id))
                {
                    lastReleaseByEnvironment.Add(item.ReleaseEnvironment.Id, new Release());
                }

                if (item.Attempts > 1 && lastReleaseByEnvironment[item.ReleaseEnvironment.Id].Name != item.Name &&
                    ReleaseVersionIsLater(item.Name, lastReleaseByEnvironment[item.ReleaseEnvironment.Id].Name))
                {
                    rolledBackReleases.Add(lastReleaseByEnvironment[item.ReleaseEnvironment.Id]);
                }

                lastReleaseByEnvironment[item.ReleaseEnvironment.Id] = item;
            }

            return rolledBackReleases;
        }


        private bool ReleaseVersionIsLater(string currentItemName, string lastItemName)
        {
            if (currentItemName == null || lastItemName == null) return false;
            if (currentItemName.Contains("TrueNorthTest-"))
            {
                var currentItemNumber = int.Parse(currentItemName.Substring(currentItemName.IndexOf("-") + 1));
                var lastItemNumber = int.Parse(lastItemName.Substring(lastItemName.IndexOf("-") + 1));
                return currentItemNumber < lastItemNumber;
            }
            else
            {
                var currentItemNumber =
                    decimal.Parse(string.Join(".", currentItemName.Split('.').Reverse().Take(2).Reverse()));
                var lastItemNumber =
                    decimal.Parse(string.Join(".", lastItemName.Split('.').Reverse().Take(2).Reverse()));
                return currentItemNumber < lastItemNumber;
            }
        }

        public static bool DevTeamForReleaseIsSelected(bool assessmentsTeam, bool enterpriseTeam, Release release)
        {
            if (release.ReleaseEnvironment.Id == 15 && assessmentsTeam)
            {
                return true;
            }

            if ((release.ReleaseEnvironment.Id == 4 || release.ReleaseEnvironment.Id == 19
                || release.ReleaseEnvironment.Id == 43 || release.ReleaseEnvironment.Id == 33) && enterpriseTeam)
            {
                return true;
            }

            return false;
        }
    }
}
