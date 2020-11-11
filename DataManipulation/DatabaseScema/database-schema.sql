create table ReleaseEnvironment
(
    Id   int not null
        constraint ReleaseEnvironment_pk
            primary key nonclustered,
    Name nvarchar(50)
)
go

create table Release
(
    Id                   int not null
        constraint Release_pk
            primary key nonclustered,
    Status               nvarchar(50),
    ReleaseEnvironmentId int
        constraint Release_ReleaseEnvironment_Id_fk
            references ReleaseEnvironment,
    StartTime            DateTimeOffset,
    FinishTime           DateTimeOffset,
    Name                 nvarchar(100),
    Attempts             int
)
go

create unique index Release_Id_uindex
    on Release (Id)
go

create unique index ReleaseEnvironment_Id_uindex
    on ReleaseEnvironment (Id)
go

create table TaskItemType
(
    Id   int not null
        constraint table_name_pk
            primary key nonclustered,
    Name nvarchar(255)
)
go

create table TaskItem
(
    Id                  int not null
        constraint TaskItem_pk
            primary key nonclustered,
    Title               nvarchar(255),
    StartTime           DateTimeOffset,
    FinishTime          DateTimeOffset,
    TaskItemTypeId      int
        constraint TaskItem_TaskItemType_Id_fk
            references TaskItemType,
    DevelopmentTeamName nvarchar(50),
    CreatedOn           DateTimeOffset,
    CreatedBy           nvarchar(50),
    LastChangedOn       DateTimeOffset,
    LastChangedBy       nvarchar(50),
    CurrentBoardColumn  nvarchar(255),
    CardState           nvarchar(255),
    Impact              nvarchar(255),
    CommentCount        int,
    NumRevisions        int,
    ReleaseId           int
        constraint TaskItem_Release_Id_fk
            references Release
)
go

create unique index TaskItem_Id_uindex
    on TaskItem (Id)
go

create table UserInfo
(
    Email     nvarchar(255) not null
        constraint UserInfo_pk
            primary key nonclustered,
    FirstName nvarchar(255),
    LastName  nvarchar(255),
    Password  nvarchar(255),
    Guid      uniqueidentifier
)
go

