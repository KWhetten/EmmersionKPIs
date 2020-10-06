create table AuthorizedUsers
(
    Guid uniqueidentifier not null
        constraint Authorization_pk
            primary key nonclustered,
    Email nvarchar(100)
        constraint Authorization_UserInfo_Email_fk
            references UserInfo,
    Expires datetime
)
go

