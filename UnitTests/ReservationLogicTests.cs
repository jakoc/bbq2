using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using BobsBBQApi.BLL;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers;
using BobsBBQApi.BE;

[TestFixture]
public class ReservationLogicTests
{
    private Mock<IReservationRepository> _reservationRepoMock;
    private Mock<ITableRepository> _tableRepoMock;
    private Mock<IUserRepository> _userRepoMock;
    private ReservationLogic _reservationLogic;

    [SetUp]
    public void Setup()
    {
        _reservationRepoMock = new Mock<IReservationRepository>();
        _tableRepoMock = new Mock<ITableRepository>();
        _userRepoMock = new Mock<IUserRepository>();

        _reservationLogic = new ReservationLogic(
            _reservationRepoMock.Object,
            _tableRepoMock.Object,
            _userRepoMock.Object
        );
    }

    [Test]
    public void GetAvailableTimeSlot_ShouldReturnAvailableSlots_WhenTablesAreFree()
    {
        var date = DateTime.Today.AddDays(1);
        var partySize = 4;
        var defaultSlots = DefaultTimeSlots.GetSlots(date);

        _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
            .Returns(new List<DateTime>());

        _tableRepoMock.Setup(t => t.GetTables())
            .Returns(new List<Table>
            {
                new Table { TableId = Guid.NewGuid(), TableNumber = 1, Capacity = 4 }
            });

        var result = _reservationLogic.GetAvailableTimeSlot(date, partySize);

        Assert.IsNotEmpty(result);
        Assert.AreEqual(defaultSlots.Count, result.Count);
    }

    [Test]
    public void GetAvailableTimeSlot_ShouldThrow_WhenNoTableCanFitPartySize()
    {
        var date = DateTime.Today.AddDays(1);
        var partySize = 10;

        _tableRepoMock.Setup(t => t.GetTables())
            .Returns(new List<Table>
            {
                new Table { TableId = Guid.NewGuid(), TableNumber = 1, Capacity = 4 }
            });

        _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
            .Returns(new List<DateTime>());

        Assert.Throws<ArgumentException>(() =>
            _reservationLogic.GetAvailableTimeSlot(date, partySize));
    }

    [Test]
    public void ReserveTable_ShouldReserve_WhenInputsAreValid()
    {
        var date = DateTime.Today.AddDays(1);
        var timeSlot = 12;
        var partySize = 4;
        var userId = Guid.NewGuid();
        var tableNumber = 1;

        var tableId = Guid.NewGuid();

        _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
            .Returns(new List<DateTime>());

        _tableRepoMock.Setup(t => t.GetTables())
            .Returns(new List<Table>
            {
                new Table { TableId = tableId , Capacity = 4 }
            });

        _reservationRepoMock.Setup(r => r.ReserveTable(It.IsAny<Reservation>()))
            .Verifiable();

        _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId);

        _reservationRepoMock.Verify(r => r.ReserveTable(It.Is<Reservation>(res =>
            res.UserId == userId &&
            res.TableId == tableId &&
            res.PartySize == partySize &&
            res.ReservationDate == date &&
            res.TimeSlot == timeSlot
        )), Times.Once);
    }

    

    [Test]
    public void ReserveTable_ShouldThrow_WhenTimeSlotUnavailable()
    {
        var date = DateTime.Today.AddDays(1);
        var invalidTimeSlot = 6; // outside default slots
        var partySize = 2;
        var userId = Guid.NewGuid();

        _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
            .Returns(DefaultTimeSlots.GetSlots(date)); // All slots taken

        _tableRepoMock.Setup(t => t.GetTables())
            .Returns(new List<Table>
            {
                new Table { TableId = Guid.NewGuid(), TableNumber = 1, Capacity = 4 }
            });

        var ex = Assert.Throws<ArgumentException>(() =>
            _reservationLogic.ReserveTable(date, invalidTimeSlot, partySize, "Note", userId));

        Assert.That(ex.Message, Is.EqualTo("Time slot must be between 10 AM and 10 PM."));
    }
    [TestCase(0)]
    [TestCase(11)]
    public void ReserveTable_ShouldThrow_WhenPartySizeOutOfRange(int partySize)
    {
        var date = DateTime.Today.AddDays(1);
        var timeSlot = 12;
        var userId = Guid.NewGuid();

        var table = new Table
        {
            TableId = Guid.NewGuid(),
            Capacity = 12
        };
    
        var availableSlots = DefaultTimeSlots.GetSlots(date);
        _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
            .Returns(new List<DateTime>()); // All slots free
        _tableRepoMock.Setup(t => t.GetTables())
            .Returns(new List<Table> { table });

        var ex = Assert.Throws<ArgumentException>(() =>
            _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId));

        Assert.That(ex.Message, Does.Contain("Party size must be between 1 and 10"));
    }
    
    [TestCase(8)] // 8 AM
    [TestCase(23)] // 11 PM
    public void ReserveTable_ShouldThrow_WhenTimeSlotOutsideOperatingHours(int hour)
    {
        var date = DateTime.Today.AddDays(1);
        var timeSlot = hour;
        var partySize = 4;
        var userId = Guid.NewGuid();
    
        _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
            .Returns(DefaultTimeSlots.GetSlots(date));
    
        _tableRepoMock.Setup(t => t.GetTables())
            .Returns(new List<Table>
            {
                new Table { TableId = Guid.NewGuid(), Capacity = 6 }
            });
    
        var ex = Assert.Throws<ArgumentException>(() =>
            _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId));
    
        Assert.That(ex.Message, Is.EqualTo("Time slot must be between 10 AM and 10 PM."));
    }
    
    [Test]
    public void ReserveTable_ShouldThrow_WhenReservationDateInPast()
    {
        var date = DateTime.Today.AddDays(-1); // Past date
        var timeSlot = 12;
        var partySize = 2;
        var userId = Guid.NewGuid();

    
        _tableRepoMock.Setup(t => t.GetTables())
            .Returns(new List<Table>
            {
                new Table { TableId = Guid.NewGuid(), Capacity = 4 }
            });
    
        _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
            .Returns(DefaultTimeSlots.GetSlots(date));
    
        var ex = Assert.Throws<ArgumentException>(() =>
            _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId));
    
        Assert.That(ex.Message, Is.EqualTo("Reservation date cannot be in the past."));
    }
    
    [Test]
    public void ReserveTable_ShouldThrow_WhenUserIdIsEmpty()
    {
        var date = DateTime.Today.AddDays(1);
        var timeSlot = 12;
        var partySize = 2;
        var emptyUserId = Guid.Empty;
    
        _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
            .Returns(DefaultTimeSlots.GetSlots(date));
    
        _tableRepoMock.Setup(t => t.GetTables())
            .Returns(new List<Table>
            {
                new Table { TableId = Guid.NewGuid(), Capacity = 4 }
            });
    
        var ex = Assert.Throws<ArgumentException>(() =>
            _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", emptyUserId));
    
        Assert.That(ex.Message, Is.EqualTo("User ID cannot be empty."));
    }

    
}

