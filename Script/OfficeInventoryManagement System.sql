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

CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL, -- e.g., Admin, SuperAdmin, Standard User
    IsActive BIT NOT NULL DEFAULT 1, 
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedOn DATETIME NOT NULL DEFAULT GETDATE()
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
	InventoryThreshold INT NOT NULL DEFAULT 0,
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
    i.InventoryThreshold,
	i.IsActive,
    i.CreatedOn,
    i.UpdatedOn
FROM 
    InventoryItem i
LEFT JOIN 
    InventoryTransactions t ON i.ItemID = t.ItemID
GROUP BY 
    i.ItemID, i.Name, i.Description, i.CategoryID, i.Quantity, i.UnitPrice, i.InventoryThreshold, i.IsActive, i.CreatedOn, i.UpdatedOn;

---------------------------

-- Inventory: Low Stock Alerts

ALTER TABLE InventoryItem
ADD InventoryThreshold INT NOT NULL DEFAULT 0;

--------------

CREATE OR ALTER PROCEDURE spGetLowStockItems
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        i.ItemID,
        i.Name,
        i.Description,
        i.CategoryID,
        c.Name AS Category,
        i.InitialQuantity,
        i.TotalQuantity,
        i.InventoryThreshold,
        i.UnitPrice,
        i.IsActive,
        i.CreatedOn,
        i.UpdatedOn
    FROM vwInventoryWithTotalQuantity i
    INNER JOIN InventoryCategory c ON i.CategoryID = c.CategoryID
    WHERE i.TotalQuantity <= i.InventoryThreshold
      AND i.IsActive = 1;
END;


--------------------------

--Inventory: Search

CREATE OR ALTER PROCEDURE spSearchInventoryItems
    @SearchTerm NVARCHAR(100) = NULL,  -- User's input to search
    @PageNumber INT = 1,               -- Pagination: page number
    @PageSize INT = 10                 -- Pagination: page size
AS
BEGIN
    SET NOCOUNT ON;

    -- Return matching items and their categories
    SELECT 
        i.ItemID,
        i.Name AS ItemName,
        i.Description,
        c.CategoryID,
        c.Name AS CategoryName,
        i.Quantity AS InitialQuantity,
        ISNULL(SUM(t.Quantity), 0) AS TotalQuantity,
        i.UnitPrice,
        i.IsActive,
        i.CreatedOn,
        i.UpdatedOn
    FROM 
        InventoryItem i
    LEFT JOIN 
        InventoryTransactions t ON i.ItemID = t.ItemID
    INNER JOIN 
        InventoryCategory c ON i.CategoryID = c.CategoryID
    WHERE
		(
            @SearchTerm IS NULL OR 
            i.Name LIKE '%' + @SearchTerm + '%' OR
            i.Description LIKE '%' + @SearchTerm + '%' OR
            c.Name LIKE '%' + @SearchTerm + '%'
        )
    GROUP BY 
        i.ItemID, i.Name, i.Description, c.CategoryID, c.Name, 
        i.Quantity, i.UnitPrice, i.IsActive, i.CreatedOn, i.UpdatedOn
    ORDER BY 
        i.Name
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;

--------------------------

-- Inventory: Batch/Lot Tracking

CREATE TABLE InventoryBatch (
    BatchID INT IDENTITY(1,1) PRIMARY KEY,    -- Unique identifier for the batch
    ItemID INT NOT NULL,                      -- Foreign key referencing InventoryItem
    BatchNumber NVARCHAR(50) NOT NULL,       -- Unique batch/lot number
    ManufactureDate DATETIME NULL,           -- Manufacture date of the batch
    ExpiryDate DATETIME NULL,                -- Expiry date for perishable items
    Quantity INT NOT NULL,                   -- Quantity for the batch
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(), -- Creation timestamp
    UpdatedOn DATETIME NOT NULL DEFAULT GETDATE(), -- Last update timestamp
    CONSTRAINT FK_InventoryBatch_Item FOREIGN KEY (ItemID) REFERENCES InventoryItem(ItemID)
);


CREATE OR ALTER PROCEDURE spAddBatch
    @ItemID INT,
    @BatchNumber NVARCHAR(50),
    @ManufactureDate DATETIME = NULL,
    @ExpiryDate DATETIME = NULL,
    @Quantity INT,
    @Status NVARCHAR(10) OUTPUT -- Returns 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO InventoryBatch (ItemID, BatchNumber, ManufactureDate, ExpiryDate, Quantity, CreatedOn, UpdatedOn)
        VALUES (@ItemID, @BatchNumber, @ManufactureDate, @ExpiryDate, @Quantity, GETDATE(), GETDATE());

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;


CREATE OR ALTER PROCEDURE spUpdateBatch
    @BatchID INT,
    @BatchNumber NVARCHAR(50),
    @ManufactureDate DATETIME = NULL,
    @ExpiryDate DATETIME = NULL,
    @Quantity INT,
    @Status NVARCHAR(10) OUTPUT -- Returns 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE InventoryBatch
        SET BatchNumber = @BatchNumber,
            ManufactureDate = @ManufactureDate,
            ExpiryDate = @ExpiryDate,
            Quantity = @Quantity,
            UpdatedOn = GETDATE()
        WHERE BatchID = @BatchID;

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;


CREATE OR ALTER PROCEDURE spGetBatchesByItem
    @ItemID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        BatchID,
        BatchNumber,
        ManufactureDate,
        ExpiryDate,
        Quantity,
        CreatedOn,
        UpdatedOn
    FROM InventoryBatch
    WHERE ItemID = @ItemID
    ORDER BY CreatedOn DESC;
END;


CREATE OR ALTER PROCEDURE spDeleteBatch
    @BatchID INT,
    @Status NVARCHAR(10) OUTPUT -- Returns 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DELETE FROM InventoryBatch
        WHERE BatchID = @BatchID;

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;

--------------------------

-- Inventory: Inventory Location Tracking

CREATE TABLE InventoryLocationTracking (
    TrackingID INT IDENTITY(1,1) PRIMARY KEY, -- Unique identifier for tracking
    ItemID INT NOT NULL,                     -- Foreign key referencing InventoryItem
    ItemName NVARCHAR(100) NOT NULL,         -- Name of the inventory item
    Quantity INT NOT NULL,                   -- Quantity being transferred
    Location NVARCHAR(255) NOT NULL,         -- Destination (warehouse, floor, department)
    UserID INT NOT NULL,                     -- User ID referencing the User table
    UserName NVARCHAR(100) NOT NULL,         -- User name referencing the User table
    TransferDate DATETIME NOT NULL DEFAULT GETDATE(), -- Date of transfer
    Notes NVARCHAR(255) NULL,                -- Optional notes about the transfer
    CONSTRAINT FK_InventoryLocationTracking_Item FOREIGN KEY (ItemID) REFERENCES InventoryItem(ItemID),
    CONSTRAINT FK_InventoryLocationTracking_User FOREIGN KEY (UserID) REFERENCES [User](UserID)
);


CREATE OR ALTER PROCEDURE spTransferInventory
    @ItemName NVARCHAR(100),
    @Quantity INT,
    @Location NVARCHAR(255),
    @UserID INT,
    @Notes NVARCHAR(255) = NULL,
    @Status NVARCHAR(10) OUTPUT -- Returns 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Step 1: Get ItemID and ensure enough stock is available
        DECLARE @ItemID INT;
        DECLARE @CurrentStock INT;
        
        SELECT 
            @ItemID = ItemID,
            @CurrentStock = ISNULL((SELECT SUM(Quantity) FROM InventoryTransactions WHERE ItemID = InventoryItem.ItemID), 0)
        FROM InventoryItem
        WHERE Name = @ItemName;

        IF (@ItemID IS NULL)
        BEGIN
            SET @Status = 'Item Not Found';
            RETURN;
        END

        IF (@CurrentStock < @Quantity)
        BEGIN
            SET @Status = 'Insufficient Stock';
            RETURN;
        END

        -- Step 2: Reduce stock in InventoryItem table
        INSERT INTO InventoryTransactions (ItemID, Quantity, TransactionType, Notes)
        VALUES (@ItemID, -@Quantity, 'StockOut', @Notes);

        -- Step 3: Track the transfer in InventoryLocationTracking
        INSERT INTO InventoryLocationTracking (ItemID, ItemName, Quantity, Location, UserID, UserName, Notes)
        SELECT 
            @ItemID, 
            @ItemName, 
            @Quantity, 
            @Location, 
            @UserID, 
            (SELECT FullName FROM [User] WHERE UserID = @UserID), 
            @Notes;

        -- Success
        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        -- Handle errors
        SET @Status = 'Failure';
    END CATCH
END;

--------------------------

-- Supplier Management: Supplier

CREATE TABLE Supplier (
    SupplierID INT IDENTITY(1,1) PRIMARY KEY, -- Unique identifier for each supplier
    Name NVARCHAR(255) NOT NULL,             -- Supplier name
    IsActive BIT DEFAULT 1,                  -- Indicates if the supplier is active
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(), -- Creation date
    UpdatedOn DATETIME NOT NULL DEFAULT GETDATE()  -- Last updated date
);


CREATE TABLE SupplierDetail (
    SDID INT IDENTITY(1,1) PRIMARY KEY,         -- Unique identifier for each detail
    SupplierID INT NOT NULL,                    -- Foreign key referencing Supplier table
    Name NVARCHAR(255) NOT NULL,               -- Contact person or branch name
    Contact1 NVARCHAR(50) NULL,                -- Primary contact
    Contact2 NVARCHAR(50) NULL,                -- Secondary contact
    Contact3 NVARCHAR(50) NULL,                -- Tertiary contact
    Contact4 NVARCHAR(50) NULL,                -- Additional contact
    Address1 NVARCHAR(255) NULL,               -- Primary address
    Address2 NVARCHAR(255) NULL,               -- Secondary address
    City NVARCHAR(100) NULL,                   -- City
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(), -- Creation date
    UpdatedOn DATETIME NOT NULL DEFAULT GETDATE(), -- Last updated date
    CONSTRAINT FK_SupplierDetail_Supplier FOREIGN KEY (SupplierID) REFERENCES Supplier(SupplierID)
);


CREATE OR ALTER PROCEDURE spCreateSupplier
    @Name NVARCHAR(255),
    @Contact1 NVARCHAR(50),
    @Contact2 NVARCHAR(50),
    @Contact3 NVARCHAR(50),
    @Contact4 NVARCHAR(50),
    @Address1 NVARCHAR(255),
    @Address2 NVARCHAR(255),
    @City NVARCHAR(100),
    @Status NVARCHAR(10) OUTPUT -- Returns 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Insert into Supplier table
        DECLARE @SupplierID INT;
        INSERT INTO Supplier (Name, IsActive, CreatedOn, UpdatedOn)
        VALUES (@Name, 1, GETDATE(), GETDATE());

        SET @SupplierID = SCOPE_IDENTITY();

        -- Insert into SupplierDetail table
        INSERT INTO SupplierDetail (SupplierID, Name, Contact1, Contact2, Contact3, Contact4, Address1, Address2, City, CreatedOn, UpdatedOn)
        VALUES (@SupplierID, @Name, @Contact1, @Contact2, @Contact3, @Contact4, @Address1, @Address2, @City, GETDATE(), GETDATE());

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;


CREATE OR ALTER PROCEDURE spUpdateSupplier
    @SupplierID INT,
    @Name NVARCHAR(255),
    @Contact1 NVARCHAR(50),
    @Contact2 NVARCHAR(50),
    @Contact3 NVARCHAR(50),
    @Contact4 NVARCHAR(50),
    @Address1 NVARCHAR(255),
    @Address2 NVARCHAR(255),
    @City NVARCHAR(100),
    @IsActive BIT = 1,
    @Status NVARCHAR(10) OUTPUT -- Returns 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Update Supplier table
        UPDATE Supplier
        SET Name = @Name,
            IsActive = @IsActive,
            UpdatedOn = GETDATE()
        WHERE SupplierID = @SupplierID;

        -- Update SupplierDetail table
        UPDATE SupplierDetail
        SET Name = @Name,
            Contact1 = @Contact1,
            Contact2 = @Contact2,
            Contact3 = @Contact3,
            Contact4 = @Contact4,
            Address1 = @Address1,
            Address2 = @Address2,
            City = @City,
            UpdatedOn = GETDATE()
        WHERE SupplierID = @SupplierID;

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;


CREATE OR ALTER PROCEDURE spGetSupplierById
    @SupplierID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        s.SupplierID, 
        s.Name AS SupplierName, 
        sd.Contact1, 
        sd.Contact2, 
        sd.Contact3, 
        sd.Contact4, 
        sd.Address1, 
        sd.Address2, 
        sd.City, 
        s.IsActive, 
        s.CreatedOn, 
        s.UpdatedOn
    FROM Supplier s
    INNER JOIN SupplierDetail sd ON s.SupplierID = sd.SupplierID
    WHERE s.SupplierID = @SupplierID AND s.IsActive = 1;
END;


CREATE OR ALTER PROCEDURE spGetAllSuppliers
    @PageNumber INT,               -- The page number to retrieve
    @PageSize INT                  -- The number of records per page
AS
BEGIN
    SET NOCOUNT ON;

    -- Fetch paginated suppliers
    SELECT 
        s.SupplierID, 
        s.Name AS SupplierName, 
        sd.Contact1, 
        sd.Contact2, 
        sd.Contact3, 
        sd.Contact4, 
        sd.Address1, 
        sd.Address2, 
        sd.City, 
        s.IsActive, 
        s.CreatedOn, 
        s.UpdatedOn
    FROM Supplier s
    INNER JOIN SupplierDetail sd ON s.SupplierID = sd.SupplierID
    WHERE s.IsActive = 1
    ORDER BY s.SupplierID
    OFFSET (@PageNumber - 1) * @PageSize ROWS -- Skip rows based on page number
    FETCH NEXT @PageSize ROWS ONLY;          -- Fetch the required number of rows
END;


CREATE INDEX IX_Supplier_IsActive ON Supplier (IsActive);
CREATE INDEX IX_SupplierDetail_SupplierID ON SupplierDetail (SupplierID);


-----------------------------

-- Purchase Order

CREATE TABLE PurchaseOrder (
    POID INT IDENTITY(1,1) PRIMARY KEY,      -- Unique identifier for the Purchase Order
    SupplierID INT NOT NULL,                -- Foreign key referencing Supplier table
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(), -- Date of the Purchase Order
    Status NVARCHAR(50) DEFAULT 'Pending',  -- Status of the PO (e.g., Pending, Approved, Received)
    TotalAmount DECIMAL(18,2) NOT NULL,     -- Total cost of the order
    Notes NVARCHAR(255) NULL,               -- Additional notes about the PO
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_PurchaseOrder_Supplier FOREIGN KEY (SupplierID) REFERENCES Supplier(SupplierID)
);


CREATE TABLE PurchaseOrderDetail (
    PODetailID INT IDENTITY(1,1) PRIMARY KEY,  -- Unique identifier for each PO detail
    POID INT NOT NULL,                         -- Foreign key referencing PurchaseOrder table
    ItemID INT NOT NULL,                       -- Foreign key referencing InventoryItem table
    Quantity INT NOT NULL,                     -- Quantity of the item ordered
    UnitPrice DECIMAL(18,2) NOT NULL,          -- Price per unit
    TotalPrice AS (Quantity * UnitPrice) PERSISTED, -- Calculated field for total price
    CONSTRAINT FK_PurchaseOrderDetail_PO FOREIGN KEY (POID) REFERENCES PurchaseOrder(POID),
    CONSTRAINT FK_PurchaseOrderDetail_Item FOREIGN KEY (ItemID) REFERENCES InventoryItem(ItemID)
);


CREATE OR ALTER PROCEDURE spCreatePurchaseOrder
    @SupplierID INT,
    @OrderDate DATETIME,
    @Notes NVARCHAR(255) = NULL,
    @ItemID INT,
    @Quantity INT,
    @UnitPrice DECIMAL(18,2),
    @Status NVARCHAR(10) OUTPUT -- Returns 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION; -- Start a transaction

        -- Validate if the SupplierID exists
        IF NOT EXISTS (SELECT 1 FROM Supplier WHERE SupplierID = @SupplierID)
        BEGIN
            SET @Status = 'Invalid SupplierID';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Insert the Purchase Order
        DECLARE @POID INT;
        INSERT INTO PurchaseOrder (SupplierID, OrderDate, Notes, TotalAmount, CreatedOn, UpdatedOn)
        VALUES (@SupplierID, @OrderDate, @Notes, @Quantity * @UnitPrice, GETDATE(), GETDATE());

        SET @POID = SCOPE_IDENTITY();

        -- Validate if the ItemID exists in InventoryItem
        IF NOT EXISTS (SELECT 1 FROM InventoryItem WHERE ItemID = @ItemID)
        BEGIN
            SET @Status = 'Invalid ItemID';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Insert details into PurchaseOrderDetail
        INSERT INTO PurchaseOrderDetail (POID, ItemID, Quantity, UnitPrice)
        VALUES (@POID, @ItemID, @Quantity, @UnitPrice);

        COMMIT TRANSACTION; -- Commit the transaction if all operations succeed
        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION; -- Rollback the transaction in case of error
        SET @Status = 'Failure';

        -- Optionally log the error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        PRINT @ErrorMessage;
    END CATCH
END;


CREATE OR ALTER PROCEDURE spUpdatePurchaseOrder
    @POID INT,
    @SupplierID INT,
    @OrderDate DATETIME,
    @Notes NVARCHAR(255) = NULL,
    @ItemID INT,
    @Quantity INT,
    @UnitPrice DECIMAL(18,2),
    @Status NVARCHAR(10) OUTPUT -- Returns 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION; -- Start a transaction

        -- Validate if the Purchase Order exists
        IF NOT EXISTS (SELECT 1 FROM PurchaseOrder WHERE POID = @POID)
        BEGIN
            SET @Status = 'Invalid POID';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate if the SupplierID exists
        IF NOT EXISTS (SELECT 1 FROM Supplier WHERE SupplierID = @SupplierID)
        BEGIN
            SET @Status = 'Invalid SupplierID';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate if the ItemID exists in InventoryItem
        IF NOT EXISTS (SELECT 1 FROM InventoryItem WHERE ItemID = @ItemID)
        BEGIN
            SET @Status = 'Invalid ItemID';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update the Purchase Order
        UPDATE PurchaseOrder
        SET SupplierID = @SupplierID,
            OrderDate = @OrderDate,
            Notes = @Notes,
            TotalAmount = @Quantity * @UnitPrice,
            UpdatedOn = GETDATE()
        WHERE POID = @POID;

        -- Update the Purchase Order Detail
        IF EXISTS (SELECT 1 FROM PurchaseOrderDetail WHERE POID = @POID AND ItemID = @ItemID)
        BEGIN
            UPDATE PurchaseOrderDetail
            SET Quantity = @Quantity,
                UnitPrice = @UnitPrice
            WHERE POID = @POID AND ItemID = @ItemID;
        END
        ELSE
        BEGIN
            INSERT INTO PurchaseOrderDetail (POID, ItemID, Quantity, UnitPrice)
            VALUES (@POID, @ItemID, @Quantity, @UnitPrice);
        END

        COMMIT TRANSACTION; -- Commit the transaction if all operations succeed
        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION; -- Rollback the transaction in case of error
        SET @Status = 'Failure';

        -- Optionally log the error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        PRINT @ErrorMessage;
    END CATCH
END;


CREATE OR ALTER PROCEDURE spGetPurchaseOrderById
    @POID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Get Purchase Order details
    SELECT POID, SupplierID, OrderDate, Notes, TotalAmount, Status, CreatedOn, UpdatedOn
    FROM PurchaseOrder
    WHERE POID = @POID;

    -- Get Purchase Order item details
    SELECT PODetailID, POID, ItemID, Quantity, UnitPrice, TotalPrice
    FROM PurchaseOrderDetail
    WHERE POID = @POID;
END;


CREATE OR ALTER PROCEDURE spGetAllPurchaseOrders
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Get paginated list of Purchase Orders
    SELECT POID, SupplierID, OrderDate, Notes, TotalAmount, Status, CreatedOn, UpdatedOn
    FROM PurchaseOrder
    ORDER BY POID
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Optionally, return total count for pagination
    SELECT COUNT(*) AS TotalCount
    FROM PurchaseOrder;
END;


---------------------
-- Create User Roles table

CREATE TABLE UserRoles (
    UserRoleID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,             -- Foreign Key referencing [User] table
    RoleID INT NOT NULL,             -- Foreign Key referencing Roles table
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserID) REFERENCES [User](UserID),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID),
    CONSTRAINT UQ_UserRole UNIQUE (UserID, RoleID) -- Avoid duplicate roles for same user
);

---------------------------
-- create Role
CREATE OR ALTER PROCEDURE spCreateRole
    @RoleName NVARCHAR(50),
    @Status NVARCHAR(10) OUTPUT -- 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF EXISTS (SELECT 1 FROM Roles WHERE RoleName = @RoleName AND IsActive = 1)
        BEGIN
            SET @Status = 'Role Exists';
            RETURN;
        END

        INSERT INTO Roles (RoleName, IsActive, CreatedOn, UpdatedOn)
        VALUES (@RoleName, 1, GETDATE(), GETDATE());

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;

---------------------------
-- update Role

CREATE OR ALTER PROCEDURE spUpdateRole
    @RoleID INT,
    @RoleName NVARCHAR(50),
    @IsActive BIT,
    @Status NVARCHAR(10) OUTPUT -- 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleID = @RoleID)
        BEGIN
            SET @Status = 'Invalid Role';
            RETURN;
        END

        UPDATE Roles
        SET RoleName = @RoleName,
            IsActive = @IsActive,
            UpdatedOn = GETDATE()
        WHERE RoleID = @RoleID;

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;

---------------------------
-- Get Roles by ID

CREATE OR ALTER PROCEDURE spGetRoleById
    @RoleID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT RoleID, RoleName, IsActive, CreatedOn, UpdatedOn
    FROM Roles
    WHERE RoleID = @RoleID;
END;

------------------------------------
-- Get all Roles

CREATE OR ALTER PROCEDURE spGetAllRoles
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Fetch paginated roles
    SELECT RoleID, RoleName, IsActive, CreatedOn, UpdatedOn
    FROM Roles
    ORDER BY RoleID
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Optionally return total count for pagination
    SELECT COUNT(*) AS TotalCount
    FROM Roles;
END;

------------------------------------
-- Assign Role to User

CREATE OR ALTER PROCEDURE spAssignRoleToUser
    @UserID INT,
    @RoleID INT,
    @Status NVARCHAR(10) OUTPUT -- 'Success', 'Failure', or 'Exists'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validate Role
        IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleID = @RoleID AND IsActive = 1)
        BEGIN
            SET @Status = 'Invalid Role';
            RETURN;
        END

        -- Validate User
        IF NOT EXISTS (SELECT 1 FROM [User] WHERE UserID = @UserID AND IsActive = 1)
        BEGIN
            SET @Status = 'Invalid User';
            RETURN;
        END

        -- Check if assignment already exists
        IF EXISTS (SELECT 1 FROM UserRoles WHERE UserID = @UserID AND RoleID = @RoleID)
        BEGIN
            SET @Status = 'Exists';
            RETURN;
        END

        INSERT INTO UserRoles (UserID, RoleID, CreatedOn, UpdatedOn)
        VALUES (@UserID, @RoleID, GETDATE(), GETDATE());

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;

-------------------------------
-- Remove Role from user

CREATE OR ALTER PROCEDURE spRemoveRoleFromUser
    @UserID INT,
    @RoleID INT,
    @Status NVARCHAR(10) OUTPUT -- 'Success' or 'Failure'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserID = @UserID AND RoleID = @RoleID)
        BEGIN
            SET @Status = 'Not Assigned';
            RETURN;
        END

        DELETE FROM UserRoles
        WHERE UserID = @UserID AND RoleID = @RoleID;

        SET @Status = 'Success';
    END TRY
    BEGIN CATCH
        SET @Status = 'Failure';
    END CATCH
END;

-------------------------------

CREATE PROCEDURE spGetAllCategory  
    @PageNumber INT,  
    @PageSize INT  
AS  
BEGIN  
    SET NOCOUNT ON;

    SELECT CategoryID, [Name]
    FROM InventoryCategory WITH (NOLOCK)
    ORDER BY CategoryID ASC
	OFFSET (@PageNumber - 1) * @PageSize ROWS    
    FETCH NEXT @PageSize ROWS ONLY;;
END;  

-------------------------------

