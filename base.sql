IF DB_ID('DocShare') IS NOT NULL
BEGIN
    ALTER DATABASE DocShare SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DocShare;
END
GO

CREATE DATABASE DocShare;
GO
USE DocShare;
GO

CREATE TABLE Universities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,  
    Code VARCHAR(50) NOT NULL UNIQUE,
    IsActive BIT DEFAULT 1,
);
GO

CREATE TABLE UniversitySections (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UniversityId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    CONSTRAINT FK_Sections_Uni FOREIGN KEY (UniversityId) REFERENCES Universities(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_UniversitySections_UniversityId ON UniversitySections(UniversityId);
GO

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NULL, 
    GoogleId VARCHAR(255),
    FullName NVARCHAR(100),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    RefreshToken VARCHAR(256),
    RefreshTokenExpiryTime DATETIME,
    Role VARCHAR(50) DEFAULT 'User' CHECK(Role IN ('User','Admin')),

    LoginProvider VARCHAR(20) NULL Check(LoginProvider In('Custom','Google')),
    CustomAvatar VARCHAR(255) DEFAULT 'default-avatar.jpg',
    GoogleAvatar VARCHAR(255),
    IsActive BIT DEFAULT 1,
    StorageLimit BIGINT NOT NULL DEFAULT 5368709120, 
    
    UsedStorage BIGINT NOT NULL DEFAULT 0,
    UniversityId INT NULL, 

    FollowerCount INT DEFAULT 0, 
    FollowingCount INT DEFAULT 0,
    CONSTRAINT FK_Users_Universities FOREIGN KEY (UniversityId) REFERENCES Universities(Id) ON DELETE SET NULL,
);
GO
CREATE TABLE UserFollows (
    FollowerId INT NOT NULL, 
    FollowedId INT NOT NULL, 
    CreatedAt DATETIME DEFAULT GETDATE(),

    PRIMARY KEY (FollowerId, FollowedId),
    CONSTRAINT CHK_NotFollowingSelf CHECK (FollowerId <> FollowedId),
    CONSTRAINT FK_Follows_Follower FOREIGN KEY (FollowerId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Follows_Followed FOREIGN KEY (FollowedId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO


CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Slug VARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);
GO

CREATE TABLE Tags (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Slug VARCHAR(50) NOT NULL 
);
GO

CREATE TABLE Documents (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    
    FileUrl NVARCHAR(500) NOT NULL,
    SizeInBytes BIGINT NOT NULL, 
    Thumbnail NVARCHAR(500),
    PageCount int ,
    
    UploaderId INT NOT NULL,
    CategoryId INT, 
    UniversitySectionId INT NULL, 

    Status VARCHAR(20) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Public', 'Private', 'Rejected')), 
    
    IsDeleted TINYINT DEFAULT 0,
    DeletedAt DATETIME NULL,          
    
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    ViewCount INT NOT NULL DEFAULT 0,
    LikeCount INT NOT NULL DEFAULT 0,
    DislikeCount INT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Docs_User FOREIGN KEY (UploaderId) REFERENCES Users(Id),
    CONSTRAINT FK_Docs_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Docs_UniSection FOREIGN KEY (UniversitySectionId) REFERENCES UniversitySections(Id) ON DELETE SET NULL
);
GO

CREATE TABLE DocumentTags (
    DocumentId BIGINT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (DocumentId, TagId), 

    CONSTRAINT FK_DocTags_Doc FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DocTags_Tag FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
);
GO

CREATE TABLE DocumentVotes (
    UserId INT NOT NULL,
    DocumentId BIGINT NOT NULL,
    IsLike BIT NOT NULL, 
    VotedAt DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (UserId, DocumentId), 

    CONSTRAINT FK_Votes_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Votes_Doc FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE
);
GO

CREATE TABLE SavedDocuments (
    UserId INT NOT NULL,
    DocumentId BIGINT NOT NULL,
    SavedAt DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (UserId, DocumentId),

    CONSTRAINT FK_Saved_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Saved_Doc FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE
);
GO

--==============================Trigger===============================
--cập nhật storage người dùng khi upload dữ liệu lên
CREATE TRIGGER trg_UpdateUsedStorage_OnInsert
ON Documents
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE u
    SET u.UsedStorage = u.UsedStorage + i.SizeInBytes
    FROM Users u INNER JOIN INSERTED i ON u.Id = i.UploaderId;
    
    IF EXISTS (SELECT 1 FROM Users u JOIN INSERTED i ON u.Id = i.UploaderId 
               WHERE u.UsedStorage > u.StorageLimit)
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50001, 'Dung lượng lưu trữ của bạn đã đầy!', 1;
    END
END;
GO
-- cập nhật storage khi người dùng xóa
CREATE TRIGGER trg_UpdateUsedStorage_OnDelete
ON Documents
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE u
    SET u.UsedStorage = u.UsedStorage - d.SizeInBytes
    FROM Users u
    INNER JOIN DELETED d ON u.Id = d.UploaderId;
END;
GO
-- cập nhật like/dislike/not
CREATE TRIGGER trg_UpdateVoteCounts
ON DocumentVotes
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AffectedDocs TABLE (DocId BIGINT);
    INSERT INTO @AffectedDocs (DocId)
    SELECT DocumentId FROM INSERTED
    UNION
    SELECT DocumentId FROM DELETED;
    UPDATE d
    SET 
        d.LikeCount = (SELECT COUNT(*) FROM DocumentVotes v WHERE v.DocumentId = d.Id AND v.IsLike = 1),
        d.DislikeCount = (SELECT COUNT(*) FROM DocumentVotes v WHERE v.DocumentId = d.Id AND v.IsLike = 0)
    FROM Documents d
    INNER JOIN @AffectedDocs ad ON d.Id = ad.DocId;
END;
GO

CREATE TRIGGER trg_UpdateFollowerFollowingCounts
ON UserFollows
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Khi có người nhấn Follow (INSERT)
    IF EXISTS (SELECT 1 FROM INSERTED)
    BEGIN
        -- Tăng FollowingCount cho người đi follow
        UPDATE u SET u.FollowingCount = u.FollowingCount + 1
        FROM Users u INNER JOIN INSERTED i ON u.Id = i.FollowerId;

        -- Tăng FollowerCount cho người được follow
        UPDATE u SET u.FollowerCount = u.FollowerCount + 1
        FROM Users u INNER JOIN INSERTED i ON u.Id = i.FollowedId;
    END

    -- Khi có người nhấn Unfollow (DELETE)
    IF EXISTS (SELECT 1 FROM DELETED)
    BEGIN
        -- Giảm FollowingCount cho người bỏ follow
        UPDATE u SET u.FollowingCount = u.FollowingCount - 1
        FROM Users u INNER JOIN DELETED d ON u.Id = d.FollowerId;

        -- Giảm FollowerCount cho người bị bỏ follow
        UPDATE u SET u.FollowerCount = u.FollowerCount - 1
        FROM Users u INNER JOIN DELETED d ON u.Id = d.FollowedId;
    END
END;
GO
CREATE INDEX IX_Tags_Slug ON Tags(Slug);
CREATE INDEX IX_Documents_IsDeleted ON Documents(IsDeleted); 
CREATE INDEX IX_Documents_UploaderId ON Documents(UploaderId);
CREATE INDEX IX_UserFollows_FollowerId ON UserFollows(FollowerId);
CREATE INDEX IX_UserFollows_FollowedId ON UserFollows(FollowedId);

Go
INSERT INTO Universities (Name, Code, IsActive) VALUES
(N'Đại học Công thương TP.HCM', 'HUIT', 1),
(N'Đại học Bách khoa - ĐHQG TP.HCM', 'HCMUT', 1),
(N'Đại học Khoa học Tự nhiên - ĐHQG TP.HCM', 'VNU-HCMUS', 1),
(N'Đại học Khoa học Xã hội và Nhân văn - ĐHQG TP.HCM', 'VNU-HCMUSSH', 1),
(N'Đại học Quốc tế - ĐHQG TP.HCM', 'VNU-HCMIU', 1),
(N'Đại học Công nghệ Thông tin - ĐHQG TP.HCM', 'UIT', 1),
(N'Đại học Kinh tế - Luật - ĐHQG TP.HCM', 'UEL', 1),
(N'Đại học Kinh tế TP.HCM', 'UEH', 1),
(N'Đại học Sư phạm Kỹ thuật TP.HCM', 'HCMUTE', 1),
(N'Đại học Y Dược TP.HCM', 'UMP', 1),
(N'Đại học Luật TP.HCM', 'ULAW', 1),
(N'Đại học Công nghiệp TP.HCM', 'IUH', 1),
(N'Đại học Nông Lâm TP.HCM', 'NLU', 1),
(N'Đại học Giao thông Vận tải TP.HCM', 'UTH', 1),
(N'Đại học Sư phạm TP.HCM', 'HCMUE', 1),
(N'Đại học Ngân hàng TP.HCM', 'HUB', 1),
(N'Đại học Tài chính - Marketing', 'UFM', 1),
(N'Đại học Sài Gòn', 'SGU', 1),
(N'Đại học Mở TP.HCM', 'OU', 1),
(N'Đại học Tôn Đức Thắng', 'TDTU', 1),
(N'Đại học Y khoa Phạm Ngọc Thạch', 'PNTU', 1),
(N'Đại học Kiến trúc TP.HCM', 'UAH', 1),
(N'Đại học Tài nguyên và Môi trường TP.HCM', 'HCMUNRE', 1),
(N'Đại học Văn hóa TP.HCM', 'VHS', 1),
(N'Đại học Thể dục Thể thao TP.HCM', 'USH', 1),
(N'Đại học Mỹ thuật TP.HCM', 'HCMUFA', 1),
(N'Nhạc viện TP.HCM', 'HCMCONS', 1),
(N'Đại học Sân khấu - Điện ảnh TP.HCM', 'SKDAHCM', 1),
(N'Học viện Cán bộ TP.HCM', 'HVCB', 1),

(N'Đại học FPT', 'FPTU', 1),
(N'Đại học RMIT Việt Nam', 'RMIT', 1),
(N'Đại học Hoa Sen', 'HSU', 1),
(N'Đại học Văn Lang', 'VLU', 1),
(N'Đại học HUTECH', 'HUTECH', 1),
(N'Đại học Ngoại ngữ - Tin học TP.HCM', 'HUFLIT', 1),
(N'Đại học Nguyễn Tất Thành', 'NTTU', 1),
(N'Đại học Kinh tế - Tài chính TP.HCM', 'UEF', 1),
(N'Đại học Quốc tế Hồng Bàng', 'HIU', 1),
(N'Đại học Gia Định', 'GDU', 1),
(N'Đại học Văn Hiến', 'VHU', 1),
(N'Đại học Quốc tế Sài Gòn', 'SIU', 1),
(N'Đại học Quản lý và Công nghệ TP.HCM', 'UMT', 1),
(N'Đại học Fulbright Việt Nam', 'FUV', 1)
