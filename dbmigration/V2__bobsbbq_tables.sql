CREATE TABLE Users (
    UserID CHAR(36) PRIMARY KEY NOT NULL, -- GUID as CHAR(36)
    UserName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    PhoneNumber INT NOT NULL,
    UserRole VARCHAR(50) NOT NULL,
    UserHash VARCHAR(100) NOT NULL,
    UserSalt VARCHAR(100) NOT NULL
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
    Note TEXT,
    FOREIGN KEY (TableID) REFERENCES RestaurantTables(TableID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);