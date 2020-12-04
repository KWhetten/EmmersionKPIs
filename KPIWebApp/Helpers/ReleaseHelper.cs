using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Objects;
using KPIWebApp.Models;

namespace KPIWebApp.Helpers
{
    public class ReleaseHelper
    {
        public OverviewData PopulateOverviewData(OverviewData overviewData, List<Release> releaseList, DateTimeOffset finishDate)
        {
            var rolledBackReleases = new List<Release>();
            var lastRelease = releaseList.First();
            DateTimeOffset? earliestReleaseFinishTime = null;
            foreach (var item in releaseList)
            {
                if (item.FinishTime < earliestReleaseFinishTime || earliestReleaseFinishTime == null)
                {
                    earliestReleaseFinishTime = item.FinishTime;
                }

                if (item.Attempts > 1 && lastRelease.Name != item.Name)
                {
                    rolledBackReleases.Add(item);
                }

                lastRelease = item;
            }
            var releaseWeeks = (finishDate - earliestReleaseFinishTime)?.Days / 7m;

            overviewData.TotalDeploys = releaseList.Count;
            overviewData.SuccessfulDeploys = releaseList.Count - rolledBackReleases.Count;
            overviewData.RolledBackDeploys = rolledBackReleases.Count;
            overviewData.DeployFrequency = decimal.Round(decimal.Parse((overviewData.TotalDeploys / releaseWeeks)
                ?.ToString("0.##") ?? ""), 2, MidpointRounding.AwayFromZero);
            overviewData.MeanTimeToRestore = 0;
            overviewData.ChangeFailPercentage = 0;

            return overviewData;
        }
    }
}
