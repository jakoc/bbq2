CREATE TABLE Users (
    UserID CHAR(36) PRIMARY KEY NOT NULL, -- GUID as CHAR(36)
    UserName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PhoneNumber INT NOT NULL,
    UserRole NVARCHAR(50) NOT NULL,
    UserHash NVARCHAR(100) NOT NULL,
    UserSalt NVARCHAR(100) NOT NULL
);


CREATE TABLE RestaurantTables (
    TableID CHAR(36) PRIMARY KEY NOT NULL,  
    Capacity INT NOT NULL,
    TableNumber INT NOT NULL
);


CREATE TABLE Reservations (
    ReservationID CHAR(36) PRIMARY KEY,  
    TableID CHAR(36) NOT NULL,                   
    ReservationDate DATE NOT NULL,
    UserID CHAR(36) NOT NULL,                    
    PartySize INT NOT NULL,
    Timeslot INT NOT NULL,
    Note NVARCHAR(MAX),
    FOREIGN KEY (TableID) REFERENCES RestaurantTables(TableID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);