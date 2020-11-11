using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;

namespace AdHocs
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var releaseRepository = new ReleaseRepository();
            var taskItemRepository = new TaskItemRepository();
            var taskItemList =
                await taskItemRepository.GetTaskItemListAsync(
                    new DateTimeOffset(new DateTime(2015, 1, 1), TimeSpan.Zero), DateTimeOffset.Now);
            var newTaskItemList = new List<TaskItem>();

            foreach (var taskItem in taskItemList)
            {
                taskItem.Release = await releaseRepository.GetFirstReleaseBeforeDateAsync(taskItem.FinishTime);
                newTaskItemList.Add(taskItem);
                Console.WriteLine($"Determined Task {taskItem.Id}'s Release is {taskItem.Release.Id}.");
            }

            await taskItemRepository.InsertTaskItemListAsync(newTaskItemList);
        }
    }
}
