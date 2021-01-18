using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class MultipleLinearRegressionAnalysisTests
    {
        [Test]
        public async Task When_getting_multiple_linear_regression_data()
        {
            var taskItems = new List<TaskItem>
            {
                new TaskItem
                {
                    CreatedBy = new Developer
                    {
                        Id = 1014,
                        Name = "Charles"
                    },
                    DevelopmentTeam = new DevelopmentTeam
                    {
                        Id = 4,
                        Name = "Enterprise"
                    },
                    Type = TaskItemType.Unanticipated,
                    CreatedOn = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 15)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 19)),
                    LastChangedOn = new DateTimeOffset(new DateTime(2021, 1, 21)),
                    NumRevisions = 1
                },
                new TaskItem
                {
                    CreatedBy = new Developer
                    {
                        Id = 1014,
                        Name = "Charles"
                    },
                    DevelopmentTeam = new DevelopmentTeam
                    {
                        Id = 4,
                        Name = "Enterprise"
                    },
                    Type = TaskItemType.Product,
                    CreatedOn = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 16)),
                    LastChangedOn = new DateTimeOffset(new DateTime(2021, 1, 17)),
                    NumRevisions = 2
                },
                new TaskItem
                {
                    CreatedBy = new Developer
                    {
                        Id = 1015,
                        Name = "Dave"
                    },
                    DevelopmentTeam = new DevelopmentTeam
                    {
                        Id = 4,
                        Name = "Enterprise"
                    },
                    Type = TaskItemType.Product,
                    CreatedOn = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 17)),
                    LastChangedOn = new DateTimeOffset(new DateTime(2021, 1, 18)),
                    NumRevisions = 3
                },
                new TaskItem
                {
                    CreatedBy = new Developer
                    {
                        Id = 1014,
                        Name = "Charles"
                    },
                    DevelopmentTeam = new DevelopmentTeam
                    {
                        Id = 4,
                        Name = "Enterprise"
                    },
                    Type = TaskItemType.Engineering,
                    CreatedOn = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 15)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 18)),
                    LastChangedOn = new DateTimeOffset(new DateTime(2021, 1, 19)),
                    NumRevisions = 4
                },
                new TaskItem
                {
                    CreatedBy = new Developer
                    {
                        Id = 1015,
                        Name = "Dave"
                    },
                    DevelopmentTeam = new DevelopmentTeam
                    {
                        Id = 4,
                        Name = "Enterprise"
                    },
                    Type = TaskItemType.Engineering,
                    CreatedOn = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 16)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 19)),
                    LastChangedOn = new DateTimeOffset(new DateTime(2021, 1, 20)),
                    NumRevisions = 3
                },
                new TaskItem
                {
                    CreatedBy = new Developer
                    {
                        Id = 1014,
                        Name = "Charles"
                    },
                    DevelopmentTeam = new DevelopmentTeam
                    {
                        Id = 4,
                        Name = "Enterprise"
                    },
                    Type = TaskItemType.Engineering,
                    CreatedOn = new DateTimeOffset(new DateTime(2021, 1, 13)),
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 15)),
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 18)),
                    LastChangedOn = new DateTimeOffset(new DateTime(2021, 1, 19)),
                    NumRevisions = 4
                }
            };

            var taskItem = new MultipleLinearRegressionTaskItem
            {
                CreatedBy = new Developer
                {
                    Id = 1014,
                    Name = "Charles"
                },
                DevTeamIsAssessments = true,
                DevTeamIsEnterprise = false,
                TimeSpentInBacklog = 5.23,
                TypeIsProduct = true,
                TypeIsEngineering = false,
                TypeIsUnanticipated = false
            };

            var mockTaskItemRepository = new Mock<TaskItemRepository>();
            mockTaskItemRepository
                .Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(taskItems);
            var multipleLinearRegressionAnalysisHelper =
                new MultipleLinearRegressionAnalysisHelper(mockTaskItemRepository.Object);

            var result2 = await multipleLinearRegressionAnalysisHelper.GetEstimation(taskItem);

            Assert.That(result2, Is.EqualTo("-8.00"));
        }
    }
}
