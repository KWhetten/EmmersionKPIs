﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;
using Microsoft.TeamFoundation.Common;

namespace DataAccess.DatabaseAccess
{
    public interface IWorkItemCardDataAccess
    {
        List<WorkItemCard> GetWorkItemCardList(DateTime startDate, DateTime endDate);
        void InsertWorkItemCardList(IEnumerable<WorkItemCard> getWorkItemCardList);
        void InsertWorkItemCard(WorkItemCard workItemCard);
        WorkItemCard GetCardById(in int cardId);
        void RemoveWorkItemCardById(int cardId);
    }

    public class WorkItemCardDataAccess : IWorkItemCardDataAccess
    {
        private readonly ReleaseDataAccess releaseDataAccess = new ReleaseDataAccess();

        public virtual List<WorkItemCard> GetWorkItemCardList(DateTime startDate, DateTime endDate)
        {
            DatabaseWrapper.GetNewConnection();
            using (DatabaseWrapper.DbConnection)
            {
                var startDateString = ($"{startDate:s}").Replace("T", " ");
                var endDateString = ($"{endDate:s}").Replace("T", " ");
                var workItemCardList = DatabaseWrapper.DbConnection
                    .Query<WorkItemCard>($"SELECT * FROM WorkItemCard " +
                                         $"WHERE FinishTime > '{startDateString}' " +
                                         $"AND FinishTime < '{endDateString}'").ToList();

                foreach (var workItemCard in workItemCardList)
                {
                    try
                    {
                        var releaseId = DatabaseWrapper.DbConnection
                            .Query<int>(
                                "SELECT ReleaseId " +
                                "FROM WorkItemCard " +
                                $"WHERE Id = {workItemCard.Id}")
                            .ToList();
                        workItemCard.Release = DatabaseWrapper.DbConnection
                            .Query<Release>("SELECT * " +
                                            "FROM Release " +
                                            $"WHERE Id = {releaseId.First()}")
                            .First();
                    }
                    catch (Exception ex)
                    {
                        workItemCard.Release = null;
                    }
                }

                return workItemCardList;
            }
        }

        public void InsertWorkItemCardList(IEnumerable<WorkItemCard> getWorkItemCardList)
        {
            DatabaseWrapper.GetNewConnection();
            using (DatabaseWrapper.DbConnection)
            {
                foreach (var item in getWorkItemCardList)
                {
                    InsertWorkItemCard(item);
                }
            }
        }

        public void InsertWorkItemCard(WorkItemCard workItemCard)
        {
            try
            {
                if (workItemCard.Release != null && workItemCard.Release.Id != 0)
                {
                    releaseDataAccess.InsertRelease(workItemCard.Release);
                }

                var sql = $"IF EXISTS(SELECT * FROM WorkItemCard WHERE Id = {workItemCard.Id}) " +
                          $"UPDATE WorkItemCard SET Title = '{workItemCard.Title.Replace("\'", "\'\'")}', " +
                          $"StartTime = {(workItemCard.StartTime != DateTime.MinValue ? $"'{workItemCard.StartTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"FinishTime = {(workItemCard.FinishTime != DateTime.MaxValue ? $"'{workItemCard.FinishTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"WorkItemCardTypeId = {(int) workItemCard.Type}, " +
                          $"DevelopmentTeamName = {(!workItemCard.DevelopmentTeamName.IsNullOrEmpty() ? $"'{workItemCard.DevelopmentTeamName}'" : "null")}, " +
                          $"CreatedOn = '{workItemCard.CreatedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"CreatedBy = {(!workItemCard.CreatedBy.IsNullOrEmpty() ? $"'{workItemCard.CreatedBy}'" : "null")}, " +
                          $"LastChangedOn = '{workItemCard.LastChangedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"LastChangedBy = {(!workItemCard.LastChangedBy.IsNullOrEmpty() ? $"'{workItemCard.LastChangedBy}'" : "null")}, " +
                          $"CurrentBoardColumn = {(!workItemCard.CurrentBoardColumn.IsNullOrEmpty() ? $"'{workItemCard.CurrentBoardColumn}'" : "null")}, " +
                          $"CardState = {(!workItemCard.CardState.IsNullOrEmpty() ? $"'{workItemCard.CardState}'" : "null")}, " +
                          $"Impact = {(!workItemCard.Impact.IsNullOrEmpty() ? $"'{workItemCard.Impact}'" : "null")}, " +
                          $"CommentCount = {workItemCard.CommentCount}, " +
                          $"NumRevisions = {workItemCard.NumRevisions}, " +
                          $"ReleaseId = {(workItemCard.Release != null && workItemCard.Release.Id != 0 ? workItemCard.Release.Id.ToString() : "null")} " +
                          $"WHERE Id = {workItemCard.Id} " +
                          "ELSE " +
                          $"INSERT INTO WorkItemCard (Id, Title, StartTime, FinishTime, WorkItemCardTypeId, DevelopmentTeamName, CreatedOn, CreatedBy, " +
                          "LastChangedOn, LastChangedBy, CurrentBoardColumn, CardState, Impact, CommentCount, NumRevisions, ReleaseId) " +
                          "VALUES (" +
                          $"{workItemCard.Id}, " +
                          $"'{workItemCard.Title.Replace("\'", "\'\'")}', " +
                          $"{(workItemCard.StartTime != DateTime.MinValue ? $"'{workItemCard.StartTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"{(workItemCard.FinishTime != DateTime.MaxValue ? $"'{workItemCard.FinishTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"{(int) workItemCard.Type}, " +
                          $"{(!workItemCard.DevelopmentTeamName.IsNullOrEmpty() ? $"'{workItemCard.DevelopmentTeamName}'" : "null")}, " +
                          $"'{workItemCard.CreatedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"{(!workItemCard.CreatedBy.IsNullOrEmpty() ? $"'{workItemCard.CreatedBy}'" : "null")}, " +
                          $"'{workItemCard.LastChangedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"{(!workItemCard.LastChangedBy.IsNullOrEmpty() ? $"'{workItemCard.LastChangedBy}'" : "null")}, " +
                          $"{(!workItemCard.CurrentBoardColumn.IsNullOrEmpty() ? $"'{workItemCard.CurrentBoardColumn}'" : "null")}, " +
                          $"{(!workItemCard.CardState.IsNullOrEmpty() ? $"'{workItemCard.CardState}'" : "null")}, " +
                          $"{(!workItemCard.Impact.IsNullOrEmpty() ? $"'{workItemCard.Impact}'" : "null")}, " +
                          $"{workItemCard.CommentCount}, " +
                          $"{workItemCard.NumRevisions}, " +
                          $"{(workItemCard.Release != null && workItemCard.Release.Id != 0 ? workItemCard.Release.Id.ToString() : "null")});";
                DatabaseWrapper.DbConnection.Execute(sql);

                Console.WriteLine($"Inserted or Updated Card: {workItemCard.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to insert card: {workItemCard.Id} - " + ex.Message);
            }
        }

        public WorkItemCard GetCardById(in int cardId)
        {
            DatabaseWrapper.GetNewConnection();
            using (DatabaseWrapper.DbConnection)
            {
                var result = DatabaseWrapper.DbConnection.Query<WorkItemCard>($"SELECT * FROM WorkItemCard WHERE Id = {cardId}")
                    .First();

                var typeId = DatabaseWrapper.DbConnection.Query<int>($"SELECT WorkItemCardTypeId FROM WorkItemCard WHERE Id = {cardId}")
                    .First();
                result.Type = DatabaseWrapper.DbConnection
                    .Query<WorkItemCardType>($"SELECT * FROM WorkItemCardType WHERE Id = {typeId}").First();

                var releaseId = DatabaseWrapper.DbConnection.Query<int>($"SELECT ReleaseId FROM WorkItemCard WHERE Id = {cardId}")
                    .First();
                result.Release = DatabaseWrapper.DbConnection
                    .Query<Release>($"SELECT * FROM Release WHERE Id = {releaseId}").First();
                var releaseEnvironmentId = DatabaseWrapper.DbConnection
                    .Query<int>($"SELECT ReleaseEnvironmentId FROM Release WHERE Id = {releaseId}").First();

                result.Release.ReleaseEnvironment = DatabaseWrapper.DbConnection
                    .Query<ReleaseEnvironment>($"SELECT * FROM ReleaseEnvironment WHERE Id = {releaseEnvironmentId}")
                    .First();

                return result;
            }
        }

        public void RemoveWorkItemCardById(int cardId)
        {
            DatabaseWrapper.GetNewConnection();
            using (DatabaseWrapper.DbConnection)
            {
                DatabaseWrapper.DbConnection.Execute($"DELETE FROM WorkItemCard WHERE Id = {cardId}");
            }
        }


    }
}
