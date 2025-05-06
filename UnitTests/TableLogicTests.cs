using System;
using System.Collections.Generic;
using BobsBBQApi.BE;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.DAL.Repositories.Interfaces;
using Moq;
using NUnit.Framework;



[TestFixture]
public class TableLogicTests
{
    private Mock<ITableRepository> _tableRepositoryMock;
    private TableLogic _tableLogic;

    [SetUp]
    public void Setup()
    {
        _tableRepositoryMock = new Mock<ITableRepository>();
        _tableLogic = new TableLogic(_tableRepositoryMock.Object);
    }

    [Test]
    public void AddTable_ShouldThrowArgumentException_WhenCapacityIsZeroOrNegative()
    {
        // Arrange
        int invalidCapacity = 0;
        int validTableNumber = 1;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            _tableLogic.AddTable(invalidCapacity, validTableNumber));
        Assert.AreEqual("Capacity and table number must be greater than zero.", ex.Message);
    }

    [Test]
    public void AddTable_ShouldThrowArgumentException_WhenTableNumberIsZeroOrNegative()
    {
        // Arrange
        int validCapacity = 4;
        int invalidTableNumber = 0;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            _tableLogic.AddTable(validCapacity, invalidTableNumber));
        Assert.AreEqual("Capacity and table number must be greater than zero.", ex.Message);
    }

    [Test]
    public void AddTable_ShouldCallAddTableMethodInRepository_WhenArgumentsAreValid()
    {
        // Arrange
        int validCapacity = 4;
        int validTableNumber = 1;
        var table = new Table
        {
            TableId = Guid.NewGuid(),
            Capacity = validCapacity,
            TableNumber = validTableNumber
        };

        // Act
        _tableLogic.AddTable(validCapacity, validTableNumber);

        // Assert
        _tableRepositoryMock.Verify(r => r.AddTable(It.Is<Table>(t =>
            t.Capacity == validCapacity && 
            t.TableNumber == validTableNumber
        )), Times.Once);
    }

    [Test]
    public void GetTables_ShouldReturnTablesFromRepository()
    {
        // Arrange
        var tables = new List<Table>
        {
            new Table { TableId = Guid.NewGuid(), Capacity = 4, TableNumber = 1 },
            new Table { TableId = Guid.NewGuid(), Capacity = 2, TableNumber = 2 }
        };
        _tableRepositoryMock.Setup(r => r.GetTables()).Returns(tables);

        // Act
        var result = _tableLogic.GetTables();

        // Assert
        Assert.AreEqual(tables.Count, result.Count());
        Assert.AreEqual(tables, result);
    }
}

