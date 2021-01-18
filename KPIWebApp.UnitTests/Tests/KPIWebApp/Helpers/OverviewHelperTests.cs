using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using KPIWebApp.Models;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class OverviewHelperTests
    {
        private readonly OverviewHelper overviewHelper = new OverviewHelper();

        [Test]
        public async Task When_getting_lead_time_overview_data()
        {
            var taskList = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Task1"
                }
            };

            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository
                .Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(taskList);

            var tempOverviewHelper = new OverviewHelper(mockTaskItemRepository.Object);

            var result = await tempOverviewHelper.GetTaskItemData(DateTimeOffset.Now, DateTimeOffset.Now);

            Assert.That(result[0].Id, Is.EqualTo(taskList[0].Id));
            Assert.That(result[0].Title, Is.EqualTo(taskList[0].Title));
        }

        [Test]
        public async Task When_getting_release_data()
        {
            var releaseList = new List<Release>
            {
                new Release
                {
                    Id = 1,
                    Name = "Release1"
                }
            };

            var mockReleaseRepository = new Mock<ReleaseRepository>();
            mockReleaseRepository
                .Setup(x => x.GetReleaseListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(releaseList);

            var tempOverviewHelper = new OverviewHelper(mockReleaseRepository.Object);

            var result = await tempOverviewHelper.GetReleaseData(DateTimeOffset.Now, DateTimeOffset.Now);

            Assert.That(result.Count, Is.EqualTo(releaseList.Count));
            Assert.That(result[0].Id, Is.EqualTo(releaseList[0].Id));
            Assert.That(result[0].Name, Is.EqualTo(releaseList[0].Name));
        }

        [Test]
        public void When_getting_task_item_overview_dat_and_the_task_list_is_empty()
        {
            var result = overviewHelper.PopulateOverviewData(new List<TaskItem>());

            Assert.That(result.AverageLeadTime, Is.EqualTo(0));
            Assert.That(result.ShortestLeadTime, Is.EqualTo(0));
            Assert.That(result.LongestLeadTime, Is.EqualTo(0));
        }

        [Test]
        public void When_populating_overview_data()
        {
            var taskList = new List<TaskItem>
            {
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 15)),
                    Type = TaskItemType.Product,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                },
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Type = TaskItemType.Engineering,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                },
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Type = TaskItemType.Unanticipated,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                }
            };

            var result = overviewHelper.PopulateOverviewData(taskList);

            Assert.That(result.AverageLeadTime, Is.EqualTo(18.67));
            Assert.That(result.LongestLeadTime, Is.EqualTo(32.0));
            Assert.That(result.ShortestLeadTime, Is.EqualTo(8.0));
            Assert.That(result.TotalCards, Is.EqualTo(3));
        }

        [Test]
        public async Task When_populating_task_item_overview_data_with_specific_types()
        {
            var taskList = new List<TaskItem>
            {
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 15)),
                    Type = TaskItemType.Product,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                },
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Type = TaskItemType.Product,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                },
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                    Type = TaskItemType.Product,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                },
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Type = TaskItemType.Engineering,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                },
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Type = TaskItemType.Unanticipated,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                }
            };

            var mockTaskItemRepository = new Mock<TaskItemRepository>();
            mockTaskItemRepository
                .Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(taskList);

            var tempOverviewHelper = new OverviewHelper(mockTaskItemRepository.Object);

            var result = await tempOverviewHelper.GetTaskItemOverviewDataAsync(
                new DateTimeOffset(new DateTime(2021, 1, 11)),
                new DateTimeOffset(new DateTime(2021, 1, 15)),
                true, false, false, true, true);

            Assert.That(result.AverageLeadTime, Is.EqualTo(24m));
            Assert.That(result.LongestLeadTime, Is.EqualTo(32m));
            Assert.That(result.ShortestLeadTime, Is.EqualTo(16m));
            Assert.That(result.TotalCards, Is.EqualTo(3));
        }

        [Test]
        public async Task When_populating_task_item_overview_data_from_a_specific_team()
        {
            var taskList = new List<TaskItem>
            {
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 15)),
                    Type = TaskItemType.Product,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                },
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Type = TaskItemType.Product,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                },
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                    Type = TaskItemType.Product,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
                },
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Type = TaskItemType.Engineering,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 5,
                    Name = "Enterprise"
                }
                },
                new TaskItem
                {
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Type = TaskItemType.Unanticipated,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 5,
                    Name = "Enterprise"
                }
                }
            };

            var mockTaskItemRepository = new Mock<TaskItemRepository>();
            mockTaskItemRepository
                .Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(taskList);

            var tempOverviewHelper = new OverviewHelper(mockTaskItemRepository.Object);

            var result = await tempOverviewHelper.GetTaskItemOverviewDataAsync(
                new DateTimeOffset(new DateTime(2021, 1, 11)),
                new DateTimeOffset(new DateTime(2021, 1, 15)),
                true, true, true, true, false);

            Assert.That(result.AverageLeadTime, Is.EqualTo(24m));
            Assert.That(result.LongestLeadTime, Is.EqualTo(32m));
            Assert.That(result.ShortestLeadTime, Is.EqualTo(16m));
            Assert.That(result.TotalCards, Is.EqualTo(3));
        }

        [Test]
        public async Task When_getting_release_overview_data()
        {
            var releaseList = new List<Release>
            {
                new Release
                {
                    Id = 1
                },
                new Release
                {
                    Id = 2
                }
            };

            var overviewData = new ReleaseOverviewData{
                ChangeFailPercentage = 4.23m,
                DeployFrequency = 5.3m,
                MeanTimeToRestore = 4.68m,
                RolledBackDeploys = 2,
                SuccessfulDeploys = 43,
                TotalDeploys = 45
            };

            var mockReleaseRepository = new Mock<ReleaseRepository>();
            mockReleaseRepository
                .Setup(x => x.GetReleaseListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(releaseList);

            var mockReleaseHelper = new Mock<ReleaseHelper>();
            mockReleaseHelper.Setup(x => x.DevTeamForReleaseIsSelected(true, true, It.IsAny<Release>()))
                .Returns(true);
            mockReleaseHelper
                .Setup(x => x.PopulateOverviewData(It.IsAny<List<Release>>(),
                    It.IsAny<DateTimeOffset>(), true, true)).Returns(overviewData);

            var tempOverviewHelper = new OverviewHelper(mockReleaseRepository.Object, mockReleaseHelper.Object);

            var result = await tempOverviewHelper.GetReleaseOverviewDataAsync(new DateTimeOffset(new DateTime(2021, 1, 11)),
                new DateTimeOffset(new DateTime(2021, 1, 11)), true, true);

            Assert.That(result.ChangeFailPercentage, Is.EqualTo(overviewData.ChangeFailPercentage));
            Assert.That(result.DeployFrequency, Is.EqualTo(overviewData.DeployFrequency));
            Assert.That(result.MeanTimeToRestore, Is.EqualTo(overviewData.MeanTimeToRestore));
            Assert.That(result.RolledBackDeploys, Is.EqualTo(overviewData.RolledBackDeploys));
            Assert.That(result.SuccessfulDeploys, Is.EqualTo(overviewData.SuccessfulDeploys));
            Assert.That(result.TotalDeploys, Is.EqualTo(overviewData.TotalDeploys));
        }
    }
}
