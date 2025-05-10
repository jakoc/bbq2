using System;
using System.Collections.Generic;
using BobsBBQApi.BE;
using BobsBBQApi.BLL;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.DAL.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace UnitTests
{
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

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _tableLogic.AddTable(invalidCapacity));
            Assert.That("Capacity and table number must be greater than zero.", Is.EqualTo(ex.Message));
        }

        [Test]
        public void AddTable_ShouldCallAddTableMethodInRepository_WhenArgumentsAreValid()
        {
            // Arrange
            int validCapacity = 4;
            var table = new Table
            {
                TableId = Guid.NewGuid(),
                Capacity = validCapacity,
            };

            // Act
            _tableLogic.AddTable(validCapacity);

            // Assert
            _tableRepositoryMock.Verify(r => r.AddTable(It.Is<Table>(t =>
                t.Capacity == validCapacity
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
            Assert.That(tables.Count, Is.EqualTo(result.Count()));
            Assert.That(tables, Is.EqualTo(result));
        }

        [Test]
        public void AddTable_ShouldAssignCorrectTableNumber_BasedOnExistingCount()
        {
            // Arrange
            var existingTables = new List<Table>
            {
                new Table { TableId = Guid.NewGuid(), Capacity = 2, TableNumber = 1 },
                new Table { TableId = Guid.NewGuid(), Capacity = 4, TableNumber = 2 }
            };

            // Mocking GetTables to return the existing tables
            _tableRepositoryMock.Setup(repo => repo.GetTables())
                .Returns(existingTables);

            Table? addedTable = null;

            // Mocking AddTable to track the added table
            _tableRepositoryMock.Setup(repo => repo.AddTable(It.IsAny<Table>()))
                .Callback<Table>(t => addedTable = t);

            int newTableCapacity = 6;

            // Act
            _tableLogic.AddTable(newTableCapacity); // Assuming this method assigns table number and adds the table

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(addedTable, Is.Not.Null);
                Assert.That(addedTable!.TableNumber, Is.EqualTo(3)); // Use ! after confirming not null
                Assert.That(addedTable.Capacity, Is.EqualTo(newTableCapacity));
            });

            // Verify AddTable was called with the expected parameters
            _tableRepositoryMock.Verify(
                r => r.AddTable(It.Is<Table>(t => t.TableNumber == 3 && t.Capacity == newTableCapacity)), Times.Once);
        }
    }
}
