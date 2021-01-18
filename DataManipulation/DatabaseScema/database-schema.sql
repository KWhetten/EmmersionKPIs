create table Developers
(
    Id   int identity
        constraint Developers_pk
            primary key nonclustered,
    Name nvarchar(max)
)
go

create unique index Developers_Id_uindex
    on Developers (Id)
go

create table DevelopmentTeams
(
    Id   int           not null
        constraint DevelopmentTeams_pk
            primary key nonclustered,
    Name nvarchar(max) not null
)
go

create unique index DevelopmentTeams_Id_uindex
    on DevelopmentTeams (Id)
go

create table HistoryEvents
(
    Id             int            not null
        primary key,
    EventDate      datetimeoffset not null,
    EventType      nvarchar(255)  not null,
    TaskItemColumn nvarchar(255)  not null,
    TaskItemState  nvarchar(255)  not null,
    TaskItemId     int            not null
        constraint HistoryEvents_TaskItem_Id_fk
            references TaskItems (Id),
    Author         nvarchar(255)
)
go

create table ReleaseEnvironments
(
    Id   int          not null
        constraint ReleaseEnvironment_pk
            primary key nonclustered,
    Name nvarchar(50) not null
)
go

create unique index ReleaseEnvironment_Id_uindex
    on ReleaseEnvironments (Id)
go

create table Releases
(
    Id                   int            not null
        constraint Release_pk
            primary key nonclustered,
    State                nvarchar(50)   not null,
    ReleaseEnvironmentId int            not null
        constraint Release_ReleaseEnvironment_Id_fk
            references ReleaseEnvironments,
    StartTime            datetimeoffset not null,
    FinishTime           datetimeoffset,
    Name                 nvarchar(100)  not null,
    Attempts             int            not null
)
go

create unique index Release_Id_uindex
    on Releases (Id)
go

create table Sessions
(
    Guid    uniqueidentifier not null
        constraint AuthorizedUsers_pk
            primary key nonclustered,
    Email   nvarchar(255)    not null
        constraint AuthorizedUsers_UserInfo_Email_fk
            references Users (Email),
    Expires datetimeoffset   not null
)
go

create unique index AuthorizedUsers_Email_uindex
    on Sessions (Email)
go

create table TaskItemTypes
(
    Id   int           not null
        constraint table_name_pk
            primary key nonclustered,
    Name nvarchar(255) not null
)
go

create table TaskItems
(
    Id                 int            not null
        constraint TaskItem_pk
            primary key nonclustered,
    Title              nvarchar(255)  not null,
    StartTime          datetimeoffset,
    FinishTime         datetimeoffset,
    TaskItemTypeId     int            not null
        constraint TaskItem_TaskItemType_Id_fk
            references TaskItemTypes,
    DevelopmentTeamId  int            not null,
    CreatedOn          datetimeoffset not null,
    CreatedById        int            not null,
    LastChangedOn      datetimeoffset not null,
    LastChangedById    int            not null,
    CurrentBoardColumn nvarchar(255)  not null,
    State              int            not null,
    NumRevisions       int            not null,
    ReleaseId          int
        constraint TaskItem_Release_Id_fk
            references Releases
)
go

create unique index TaskItem_Id_uindex
    on TaskItems (Id)
go

create table Users
(
    Email     nvarchar(255) not null
        constraint UserInfo_pk
            primary key nonclustered,
    FirstName nvarchar(255) not null,
    LastName  nvarchar(255) not null,
    Password  nvarchar(255)
)
go

insert into Users (Email, FirstName, LastName, Password)
values ('testemail@email.com', 'KPI', 'Test', '$2a$10$Sl6OO0m24AaL3Z4VDrj9p.9472x.M2J.nb4KNpyBb7i7m5ihindZ2');
