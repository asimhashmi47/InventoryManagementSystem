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

----

CREATE TABLE InventoryCategory (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,  -- Unique identifier for the category
    Name NVARCHAR(100) NOT NULL UNIQUE         -- Category name (e.g., Office Supplies, IT Equipment)
);


CREATE TABLE InventoryItem (
    ItemID INT IDENTITY(1,1) PRIMARY KEY,          -- Unique identifier for each item
    Name NVARCHAR(100) NOT NULL,                  -- Name of the inventory item
    Description NVARCHAR(255) NULL,               -- Optional description
    CategoryID INT NOT NULL,                      -- Foreign key referencing InventoryCategory
    Quantity INT NOT NULL DEFAULT 0,              -- Current stock level
    UnitPrice DECIMAL(18, 2) NOT NULL,            -- Price per unit
    IsActive BIT DEFAULT 1,                       -- Indicates whether the item is active
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),-- Date of creation
    UpdatedOn DATETIME NOT NULL DEFAULT GETDATE(),-- Date of last update
    CONSTRAINT FK_InventoryItem_Category FOREIGN KEY (CategoryID) REFERENCES InventoryCategory(CategoryID)
);


CREATE OR ALTER PROCEDURE spCreateInventoryItem
    @Name NVARCHAR(100),
    @Description NVARCHAR(255),
    @CategoryID INT,
    @Quantity INT,
    @UnitPrice DECIMAL(18, 2),
    @Status NVARCHAR(10) OUTPUT -- Returns 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO InventoryItem (Name, Description, CategoryID, Quantity, UnitPrice, CreatedOn, UpdatedOn)
        VALUES (@Name, @Description, @CategoryID, @Quantity, @UnitPrice, GETDATE(), GETDATE());

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;


CREATE OR ALTER PROCEDURE spUpdateInventoryItem
    @ItemID INT,
    @Name NVARCHAR(100),
    @Description NVARCHAR(255),
    @CategoryID INT,
    @Quantity INT,
    @UnitPrice DECIMAL(18, 2),
    @Status NVARCHAR(10) OUTPUT -- Returns 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE InventoryItem
        SET Name = @Name,
            Description = @Description,
            CategoryID = @CategoryID,
            Quantity = @Quantity,
            UnitPrice = @UnitPrice,
            UpdatedOn = GETDATE()
        WHERE ItemID = @ItemID;

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;


CREATE OR ALTER PROCEDURE spGetInventoryItemById
    @ItemID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT i.ItemID, i.Name, i.Description, i.CategoryID, c.Name AS Category, i.Quantity, i.UnitPrice, i.IsActive, i.CreatedOn, i.UpdatedOn
    FROM InventoryItem i
    INNER JOIN InventoryCategory c ON i.CategoryID = c.CategoryID
    WHERE i.ItemID = @ItemID
      AND i.IsActive = 1;
END;


CREATE OR ALTER PROCEDURE spGetAllInventoryItems
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT i.ItemID, i.Name, i.Description, i.CategoryID, c.Name AS Category, i.Quantity, i.UnitPrice, i.IsActive, i.CreatedOn, i.UpdatedOn
    FROM InventoryItem i
    INNER JOIN InventoryCategory c ON i.CategoryID = c.CategoryID
    WHERE i.IsActive = 1
    ORDER BY i.ItemID
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;


----------------------------
-- Track stock levels (quantity in/out)
-- 1. Create InventoryTransactions Table

CREATE TABLE InventoryTransactions (
    TransactionID INT IDENTITY(1,1) PRIMARY KEY,    -- Unique identifier for the transaction
    ItemID INT NOT NULL,                           -- References the InventoryItem table
    Quantity INT NOT NULL,                         -- Positive for IN, Negative for OUT
    TransactionType NVARCHAR(50) NOT NULL,         -- 'StockIn' or 'StockOut'
    TransactionDate DATETIME NOT NULL DEFAULT GETDATE(), -- Date of transaction
    Notes NVARCHAR(255) NULL,                     -- Optional notes about the transaction
    CONSTRAINT FK_InventoryTransactions_Item FOREIGN KEY (ItemID) REFERENCES InventoryItem(ItemID)
);

-----------------
--Track stock levels (quantity in/out)
-- Add Stock (Stock-In)

CREATE OR ALTER PROCEDURE spStockIn
    @ItemID INT,
    @Quantity INT,
    @Notes NVARCHAR(255) = NULL,
    @Status NVARCHAR(10) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Log the transaction
        INSERT INTO InventoryTransactions (ItemID, Quantity, TransactionType, Notes)
        VALUES (@ItemID, @Quantity, 'StockIn', @Notes);

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;

-----------------
--Track stock levels (quantity in/out)
-- Remove Stock (Stock-Out)

CREATE OR ALTER PROCEDURE spStockOut
    @ItemID INT,
    @Quantity INT,
    @Notes NVARCHAR(255) = NULL,
    @Status NVARCHAR(10) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validate sufficient stock
        IF ((SELECT ISNULL(SUM(Quantity), 0) FROM InventoryTransactions WHERE ItemID = @ItemID) < @Quantity)
        BEGIN
            SET @Status = 'Insufficient Stock';
            RETURN;
        END

        -- Log the transaction
        INSERT INTO InventoryTransactions (ItemID, Quantity, TransactionType, Notes)
        VALUES (@ItemID, -@Quantity, 'StockOut', @Notes);

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;

---------------------------

--Track stock levels (quantity in/out)
-- CREATE view Inventory With Total Quantity

CREATE OR ALTER VIEW vwInventoryWithTotalQuantity AS
SELECT 
    i.ItemID,
    i.Name,
    i.Description,
    i.CategoryID,
    i.Quantity AS InitialQuantity, -- Static quantity if any
    ISNULL(SUM(t.Quantity), 0) AS TotalQuantity, -- Dynamic sum from transactions
    i.UnitPrice,
    i.IsActive,
    i.CreatedOn,
    i.UpdatedOn
FROM 
    InventoryItem i
LEFT JOIN 
    InventoryTransactions t ON i.ItemID = t.ItemID
GROUP BY 
    i.ItemID, i.Name, i.Description, i.CategoryID, i.Quantity, i.UnitPrice, i.IsActive, i.CreatedOn, i.UpdatedOn;

---------------------------
