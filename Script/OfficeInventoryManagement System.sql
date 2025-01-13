-- Create the database
CREATE DATABASE OfficeInventoryDB;

-- Use the newly created database
USE OfficeInventoryDB;

-- Create the User table
CREATE TABLE [dbo].[User] (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    RoleID INT NOT NULL,
	IsActive BIT DEFAULT 1,
    CreatedOn DATE NOT NULL DEFAULT GETDATE(),
    UpdatedOn DATE NOT NULL DEFAULT GETDATE()
);


-- SP Create User
CREATE OR ALTER PROCEDURE spCreateUser
    @FullName NVARCHAR(100),
    @Email NVARCHAR(100),
    @Password NVARCHAR(100),
    @RoleID INT,
    @IsActive BIT = 1, -- Default value for active users
    @Status NVARCHAR(10) OUTPUT -- Output parameter for success or failure or Exist status
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Check if the email already exists
        IF EXISTS (SELECT 1 FROM [User] WHERE Email = @Email)
        BEGIN
            SET @Status = 'Exist';
            RETURN;
        END;

        -- Insert the new user
        INSERT INTO [User] (FullName, Email, Password, RoleID, IsActive, CreatedOn, UpdatedOn)
        VALUES (@FullName, @Email, @Password, @RoleID, @IsActive, GETDATE(), GETDATE());

        -- Set the success status
        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        -- Log the error and return failure status
        SET @Status = 'Failure';
        THROW; -- Optional: re-throw the error for debugging
    END CATCH
END;


-- SP Update User
CREATE OR ALTER PROCEDURE spUpdateUser
    @UserID INT,
    @FullName NVARCHAR(100),
    @Email NVARCHAR(100),
    @Password NVARCHAR(100),
    @RoleID INT,
	@IsActive BIT,
    @Status NVARCHAR(10) OUTPUT -- Output parameter for success or failure status
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Check if the user exists
        IF NOT EXISTS (SELECT 1 FROM [User] WHERE UserID = @UserID)
        BEGIN
            SET @Status = 'Failure';
            RETURN;
        END;

        -- Update the user
        UPDATE [User]
        SET FullName = @FullName,
            Email = @Email,
            Password = @Password,
            RoleID = @RoleID,
			IsActive = @IsActive,
            UpdatedOn = GETDATE()
        WHERE UserID = @UserID;

        -- Set the success status
        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        -- Log the error and return failure status
        SET @Status = 'Failure';
        THROW; -- Optional: re-throw the error for debugging
    END CATCH
END;


-- SP Get User
CREATE OR ALTER PROCEDURE spGetUserById
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.UserID,
        u.FullName,
        u.Email,
        u.Password,
		r.RoleID,
        r.Role AS Role, 
        u.IsActive,
        u.CreatedOn,
        u.UpdatedOn
    FROM [User] u
    INNER JOIN Role r ON u.RoleID = r.RoleID
    WHERE u.UserID = @UserID;
END;

CREATE OR ALTER PROCEDURE spGetAllActiveUsers
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.UserID,
        u.FullName,
        u.Email,
        u.Password,
        u.RoleID,
        r.Role AS Role,
        u.IsActive,
        u.CreatedOn,
        u.UpdatedOn
    FROM [User] u
    INNER JOIN Role r ON u.RoleID = r.RoleID
    WHERE u.IsActive = 1
    ORDER BY u.UserID
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;



--Optimize Database Queries
--Indexes: Add indexes to frequently queried columns, such as IsActive and UserID, to speed up lookups.
CREATE NONCLUSTERED INDEX IX_User_IsActive ON [dbo].[User] (IsActive);


-- Table Log Exception
CREATE TABLE ExceptionLogs (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    FunctionName NVARCHAR(100),
    ErrorMessage NVARCHAR(MAX),
    StackTrace NVARCHAR(MAX),
    LogDate DATETIME
);


-- sp log exception 
CREATE PROCEDURE spLogException
    @FunctionName NVARCHAR(100),
    @ErrorMessage NVARCHAR(MAX),
    @StackTrace NVARCHAR(MAX),
    @LogDate DATETIME
AS
BEGIN
    INSERT INTO ExceptionLogs (FunctionName, ErrorMessage, StackTrace, LogDate)
    VALUES (@FunctionName, @ErrorMessage, @StackTrace, @LogDate);
END;


--
CREATE TABLE Role (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    Role NVARCHAR(50) NOT NULL UNIQUE
);

-- Insert predefined roles
INSERT INTO Role (Role) VALUES 
('SuperAdmin'),
('Admin'),
('Standard User');
